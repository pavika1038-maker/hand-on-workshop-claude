// SF-007 / SF-008: ยกเลิกคำร้องขอลา (Pending = ยกเลิกทันที, Approved = ส่งคำขอยกเลิกให้ Manager)
import { useState, useEffect, useRef } from 'react'
import { apiFetch } from '../../api'
import type { ApiResponse } from '../../types/leaveRequest'

export interface CancelTarget {
  leaveRequestId: string
  leaveRequestRef: string
  status: string
}

interface Props {
  target: CancelTarget
  onClose: () => void
  onSuccess: (message: string) => void
}

export default function CancelLeaveModal({ target, onClose, onSuccess }: Props) {
  const isApproved = target.status === 'Approved'
  const [reason, setReason]         = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError]           = useState<string | null>(null)
  const reasonRef = useRef<HTMLTextAreaElement>(null)

  useEffect(() => { setTimeout(() => reasonRef.current?.focus(), 60) }, [])
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape' && !submitting) onClose() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [submitting, onClose])

  const handleConfirm = async () => {
    // SF-008: คำร้องที่ Approved แล้ว ต้องระบุเหตุผลการยกเลิก (ส่งให้ Manager พิจารณา)
    if (isApproved && !reason.trim()) {
      setError('กรุณาระบุเหตุผลในการขอยกเลิก')
      reasonRef.current?.focus()
      return
    }
    setSubmitting(true); setError(null)
    try {
      const res = await apiFetch(`/api/v1/leave-requests/${target.leaveRequestId}/cancel`, {
        method: 'PATCH',
        body: JSON.stringify({ comment: reason.trim() || null }),
      })
      const json = await res.json() as ApiResponse<{ message?: string }>
      if (json.success) {
        onSuccess(json.data?.message ?? 'ดำเนินการสำเร็จ')
      } else {
        setError(json.message ?? 'ยกเลิกไม่สำเร็จ')
      }
    } catch {
      setError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div style={s.overlay} onClick={e => { if (e.target === e.currentTarget && !submitting) onClose() }}>
      <div style={s.box} role="dialog" aria-modal="true">
        <h3 style={s.title}>ยืนยันการยกเลิกคำร้อง</h3>
        <p style={s.ref}>{target.leaveRequestRef}</p>
        <p style={s.desc}>
          {isApproved
            ? 'คำร้องนี้อนุมัติแล้ว — การยกเลิกจะส่งเป็นคำขอให้ผู้บังคับบัญชาพิจารณาอีกครั้ง'
            : 'ต้องการยกเลิกคำร้องนี้ใช่หรือไม่? การยกเลิกจะมีผลทันที'}
        </p>

        <div style={s.field}>
          <label style={s.label}>
            เหตุผลการยกเลิก {isApproved ? <span style={{ color: '#dc2626' }}>*</span> : '(ถ้ามี)'}
          </label>
          <textarea
            ref={reasonRef} style={s.textarea} rows={3} maxLength={500} value={reason}
            onChange={e => setReason(e.target.value)} disabled={submitting}
            placeholder={isApproved ? 'ระบุเหตุผลที่ต้องการยกเลิก...' : 'ระบุเหตุผล (ถ้ามี)'}
          />
        </div>

        {error && <div style={s.errorBox}>{error}</div>}

        <div style={s.actions}>
          <button style={s.btnSecondary} onClick={onClose} disabled={submitting}>ปิด</button>
          <button style={s.btnDanger} onClick={handleConfirm} disabled={submitting}>
            {submitting ? 'กำลังดำเนินการ...' : isApproved ? 'ส่งคำขอยกเลิก' : 'ยืนยันยกเลิก'}
          </button>
        </div>
      </div>
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  overlay:   { position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 },
  box:       { backgroundColor: '#fff', borderRadius: 10, padding: '28px 32px', width: 440, maxWidth: '90vw', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' },
  title:     { fontSize: 16, fontWeight: 700, margin: 0, color: '#b91c1c' },
  ref:       { fontSize: 13, color: 'var(--color-text-muted)', margin: '6px 0 0' },
  desc:      { fontSize: 13, color: 'var(--color-text)', margin: '12px 0 16px', lineHeight: 1.6 },
  field:     { display: 'flex', flexDirection: 'column', gap: 4 },
  label:     { fontSize: 12, fontWeight: 500, color: 'var(--color-text-muted)' },
  textarea:  { padding: '8px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, fontFamily: 'inherit', resize: 'vertical' },
  errorBox:  { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626', marginTop: 12 },
  actions:   { display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 16 },
  btnSecondary: { padding: '8px 20px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnDanger:    { padding: '8px 20px', backgroundColor: '#dc2626', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
}
