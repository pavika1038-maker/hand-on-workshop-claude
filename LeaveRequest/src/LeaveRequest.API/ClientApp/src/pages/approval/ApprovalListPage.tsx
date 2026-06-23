// SCR-004: Manager Approval Inbox (SFR-004, SFR-005)
// SCR-007: Manager Approve/Reject Cancel Requests (SFR-009)
import { useState, useEffect, useCallback, useRef } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { apiGet, apiFetch } from '../../api'
import type { ApiResponse, PagedResult } from '../../types/leaveRequest'
import { formatDate } from '../../types/leaveRequest'

function slaCountdown(deadline: string): string {
  const diff = new Date(deadline).getTime() - Date.now()
  if (diff <= 0) return 'หมดเวลา'
  const h = Math.floor(diff / 3_600_000)
  const m = Math.floor((diff % 3_600_000) / 60_000)
  return `${h}:${String(m).padStart(2, '0')} ชม.`
}

interface PendingApproval {
  leaveRequestId: string
  leaveRequestRef: string
  employeeId: string
  employeeFullNameTh: string
  leaveTypeName: string
  startDate: string
  endDate: string
  durationDays: number
  reason?: string
  createdAt: string
}

interface PendingCancelRequest {
  cancelRequestId: string
  leaveRequestId: string
  leaveRequestRef: string
  employeeId: string
  employeeFullNameTh: string
  leaveTypeName: string
  startDate: string
  endDate: string
  durationDays: number
  cancelReason?: string
  cancelRequestedAt: string
  slaDeadline: string
}

type Tab = 'leave' | 'cancel'
type ModalAction = 'approve' | 'reject'
interface ModalTarget { id: string; type: Tab; label: string }

const PAGE_SIZE = 15

export default function ApprovalListPage() {
  const { user } = useAuth()
  const managerId = user?.employeeId ?? ''

  const [tab, setTab] = useState<Tab>('leave')

  // Leave approvals
  const [leaveItems, setLeaveItems]       = useState<PendingApproval[]>([])
  const [leavePage, setLeavePage]         = useState(1)
  const [leaveTotalPages, setLeaveTotalPages] = useState(1)
  const [leaveTotalCount, setLeaveTotalCount] = useState(0)
  const [loadingLeave, setLoadingLeave]   = useState(true)
  const [leaveError, setLeaveError]       = useState<string | null>(null)
  const [leaveRefresh, setLeaveRefresh]   = useState(0)

  // Cancel requests
  const [cancelItems, setCancelItems]     = useState<PendingCancelRequest[]>([])
  const [cancelPage]                      = useState(1)
  const [, setCancelTotalPages]           = useState(1)
  const [cancelTotalCount, setCancelTotalCount] = useState(0)
  const [loadingCancel, setLoadingCancel] = useState(true)
  const [cancelError, setCancelError]     = useState<string | null>(null)
  const [cancelRefresh, setCancelRefresh] = useState(0)

  // Modal
  const [modal, setModal]         = useState<ModalTarget | null>(null)
  const [modalAction, setModalAction] = useState<ModalAction | null>(null)
  const [comment, setComment]     = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)
  const [successMsg, setSuccessMsg] = useState<string | null>(null)
  const commentRef = useRef<HTMLTextAreaElement>(null)

  const isModalOpen = modal !== null && modalAction !== null

  useEffect(() => {
    if (isModalOpen) setTimeout(() => commentRef.current?.focus(), 60)
  }, [isModalOpen])

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape' && !submitting) closeModal() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [isModalOpen, submitting])

  // Load leave approvals
  const loadLeave = useCallback(async () => {
    if (!managerId) return
    setLoadingLeave(true); setLeaveError(null)
    try {
      const qs = new URLSearchParams({ managerId, page: String(leavePage), pageSize: String(PAGE_SIZE) })
      const json = await apiGet<ApiResponse<PagedResult<PendingApproval>>>(`/api/v1/approvals/pending?${qs}`)
      if (json.success && json.data) {
        setLeaveItems(json.data.items)
        setLeaveTotalCount(json.data.totalCount)
        setLeaveTotalPages(json.data.totalPages)
      } else {
        setLeaveError(json.message ?? 'ไม่สามารถโหลดรายการได้')
      }
    } catch { setLeaveError('เกิดข้อผิดพลาดในการเชื่อมต่อ') }
    finally  { setLoadingLeave(false) }
  }, [managerId, leavePage, leaveRefresh])

  useEffect(() => { loadLeave() }, [loadLeave])

  // Load cancel requests
  const loadCancel = useCallback(async () => {
    if (!managerId) return
    setLoadingCancel(true); setCancelError(null)
    try {
      const qs = new URLSearchParams({ managerId, page: String(cancelPage), pageSize: String(PAGE_SIZE) })
      const json = await apiGet<ApiResponse<PagedResult<PendingCancelRequest>>>(`/api/v1/approvals/cancel-requests?${qs}`)
      if (json.success && json.data) {
        setCancelItems(json.data.items)
        setCancelTotalCount(json.data.totalCount)
        setCancelTotalPages(json.data.totalPages)
      } else {
        setCancelError(json.message ?? 'ไม่สามารถโหลดรายการได้')
      }
    } catch { setCancelError('เกิดข้อผิดพลาดในการเชื่อมต่อ') }
    finally  { setLoadingCancel(false) }
  }, [managerId, cancelPage, cancelRefresh])

  useEffect(() => { loadCancel() }, [loadCancel])

  const openModal = (id: string, type: Tab, label: string, action: ModalAction) => {
    setModal({ id, type, label }); setModalAction(action)
    setComment(''); setActionError(null)
  }
  const closeModal = () => { setModal(null); setModalAction(null) }

  const handleAction = async () => {
    if (!modal || !modalAction) return
    if (modalAction === 'reject' && !comment.trim()) {
      setActionError('กรุณาระบุเหตุผลในการปฏิเสธ')
      commentRef.current?.focus()
      return
    }
    setSubmitting(true); setActionError(null)
    try {
      let url: string
      if (modal.type === 'leave') {
        url = `/api/v1/approvals/${modal.id}/${modalAction}`
      } else {
        url = `/api/v1/approvals/cancel-requests/${modal.id}/${modalAction}`
      }
      const res  = await apiFetch(url, { method: 'PATCH', body: JSON.stringify({ comment: comment.trim() || null }) })
      const json = await res.json() as ApiResponse<unknown>
      if (json.success) {
        const label = modalAction === 'approve' ? 'อนุมัติ' : 'ปฏิเสธ'
        setSuccessMsg(`${label}สำเร็จ`)
        closeModal()
        if (modal.type === 'leave') setLeaveRefresh(v => v + 1)
        else setCancelRefresh(v => v + 1)
        setTimeout(() => setSuccessMsg(null), 3000)
      } else {
        setActionError(json.message ?? 'ดำเนินการไม่สำเร็จ')
      }
    } catch {
      setActionError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span>อนุมัติ/ปฏิเสธการลา</span>
      </nav>

      {successMsg && <div style={s.successBanner}>{successMsg}</div>}

      {/* Tabs */}
      <div style={s.tabs}>
        <button style={{ ...s.tab, ...(tab === 'leave' ? s.tabActive : {}) }} onClick={() => setTab('leave')}>
          คำร้องขอลา {leaveTotalCount > 0 && <span style={s.tabBadge}>{leaveTotalCount}</span>}
        </button>
        <button style={{ ...s.tab, ...(tab === 'cancel' ? s.tabActive : {}) }} onClick={() => setTab('cancel')}>
          คำขอยกเลิก {cancelTotalCount > 0 && <span style={s.tabBadge}>{cancelTotalCount}</span>}
        </button>
      </div>

      <section style={s.card}>
        {tab === 'leave' ? (
          <>
            <h2 style={s.cardTitle}>รายการรออนุมัติ</h2>
            {loadingLeave ? <div style={s.center}>⏳ กำลังโหลด...</div>
            : leaveError ? <div style={s.errorBox}>{leaveError} <button style={s.btnLink} onClick={loadLeave}>ลองใหม่</button></div>
            : leaveItems.length === 0 ? <div style={s.center}>✅ ไม่มีรายการรออนุมัติ</div>
            : (
              <>
                <div style={{ overflowX: 'auto' }}>
                  <table style={s.table}>
                    <thead><tr>
                      {['พนักงาน','ประเภทการลา','วันที่','วัน','เหตุผล','วันที่ยื่น','Action'].map(h => (
                        <th key={h} style={s.th}>{h}</th>
                      ))}
                    </tr></thead>
                    <tbody>
                      {leaveItems.map(item => (
                        <tr key={item.leaveRequestId} style={s.tr}>
                          <td style={s.td}>
                            <div style={{ fontWeight: 500 }}>{item.employeeFullNameTh}</div>
                            <div style={{ fontSize: 11, color: 'var(--color-text-muted)' }}>{item.leaveRequestRef}</div>
                          </td>
                          <td style={s.td}>{item.leaveTypeName}</td>
                          <td style={{ ...s.td, whiteSpace: 'nowrap' }}>
                            {formatDate(item.startDate)}{item.startDate !== item.endDate ? ` – ${formatDate(item.endDate)}` : ''}
                          </td>
                          <td style={{ ...s.td, textAlign: 'center' }}>{item.durationDays}</td>
                          <td style={{ ...s.td, maxWidth: 160 }}>
                            <span style={s.ellipsis}>{item.reason ?? '—'}</span>
                          </td>
                          <td style={{ ...s.td, whiteSpace: 'nowrap', color: 'var(--color-text-muted)' }}>
                            {formatDate(item.createdAt)}
                          </td>
                          <td style={s.td}>
                            <div style={{ display: 'flex', gap: 6 }}>
                              <button style={s.btnApprove} onClick={() => openModal(item.leaveRequestId, 'leave', item.leaveRequestRef, 'approve')}>อนุมัติ</button>
                              <button style={s.btnReject}  onClick={() => openModal(item.leaveRequestId, 'leave', item.leaveRequestRef, 'reject')}>ปฏิเสธ</button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {leaveTotalPages > 1 && (
                  <div style={s.pagination}>
                    <button style={s.pageBtn} disabled={leavePage === 1} onClick={() => setLeavePage(p => p - 1)}>‹</button>
                    <span style={{ fontSize: 13 }}>หน้า {leavePage} / {leaveTotalPages}</span>
                    <button style={s.pageBtn} disabled={leavePage === leaveTotalPages} onClick={() => setLeavePage(p => p + 1)}>›</button>
                  </div>
                )}
              </>
            )}
          </>
        ) : (
          <>
            <h2 style={s.cardTitle}>คำขอยกเลิกรออนุมัติ</h2>
            {loadingCancel ? <div style={s.center}>⏳ กำลังโหลด...</div>
            : cancelError ? <div style={s.errorBox}>{cancelError}</div>
            : cancelItems.length === 0 ? <div style={s.center}>✅ ไม่มีคำขอยกเลิก</div>
            : (
              <div style={{ overflowX: 'auto' }}>
                <table style={s.table}>
                  <thead><tr>
                    {['พนักงาน','ประเภทการลา','ช่วงวันลา','เหตุผลยกเลิก','SLA Deadline','Action'].map(h => (
                      <th key={h} style={s.th}>{h}</th>
                    ))}
                  </tr></thead>
                  <tbody>
                    {cancelItems.map(item => (
                      <tr key={item.cancelRequestId} style={s.tr}>
                        <td style={s.td}>
                          <div style={{ fontWeight: 500 }}>{item.employeeFullNameTh}</div>
                          <div style={{ fontSize: 11, color: 'var(--color-text-muted)' }}>{item.leaveRequestRef}</div>
                        </td>
                        <td style={s.td}>{item.leaveTypeName}</td>
                        <td style={{ ...s.td, whiteSpace: 'nowrap' }}>
                          {formatDate(item.startDate)} – {formatDate(item.endDate)} ({item.durationDays} วัน)
                        </td>
                        <td style={{ ...s.td, maxWidth: 160 }}>
                          <span style={s.ellipsis}>{item.cancelReason ?? '—'}</span>
                        </td>
                        <td style={{ ...s.td, whiteSpace: 'nowrap', color: new Date(item.slaDeadline) < new Date() ? '#dc2626' : '#d97706' }}>
                          {slaCountdown(item.slaDeadline)}
                        </td>
                        <td style={s.td}>
                          <div style={{ display: 'flex', gap: 6 }}>
                            <button style={s.btnApprove} onClick={() => openModal(item.cancelRequestId, 'cancel', item.leaveRequestRef, 'approve')}>อนุมัติยกเลิก</button>
                            <button style={s.btnReject}  onClick={() => openModal(item.cancelRequestId, 'cancel', item.leaveRequestRef, 'reject')}>ปฏิเสธ</button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </>
        )}
      </section>

      {/* Modal */}
      {isModalOpen && (
        <div style={s.overlay} onClick={e => { if (e.target === e.currentTarget && !submitting) closeModal() }}>
          <div style={s.modalBox} role="dialog">
            <h3 style={{ ...s.modalTitle, color: modalAction === 'approve' ? '#15803d' : '#dc2626' }}>
              {modalAction === 'approve' ? '✅ ยืนยันการอนุมัติ' : '❌ ยืนยันการปฏิเสธ'}
            </h3>
            <p style={s.modalRef}>{modal!.label}</p>
            <div style={s.field}>
              <label style={s.label}>
                {modalAction === 'reject' ? 'เหตุผลการปฏิเสธ *' : 'หมายเหตุ (ถ้ามี)'}
              </label>
              <textarea ref={commentRef} style={s.textarea} rows={3} value={comment}
                onChange={e => setComment(e.target.value)} disabled={submitting}
                placeholder={modalAction === 'reject' ? 'กรุณาระบุเหตุผล...' : 'ระบุหมายเหตุ (ถ้ามี)'} />
            </div>
            {actionError && <div style={s.errorBox}>{actionError}</div>}
            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 16 }}>
              <button style={s.btnSecondary} onClick={closeModal} disabled={submitting}>ยกเลิก</button>
              <button
                style={modalAction === 'approve' ? s.btnApprove : s.btnRejectLg}
                onClick={handleAction} disabled={submitting}>
                {submitting ? 'กำลังดำเนินการ...' : modalAction === 'approve' ? 'อนุมัติ' : 'ปฏิเสธ'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  breadcrumb: { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 16, fontSize: 13 },
  crumbLink:  { color: 'var(--color-primary)' },
  crumbSep:   { color: 'var(--color-text-muted)', margin: '0 4px' },
  successBanner: { backgroundColor: '#e8f5e9', border: '1px solid #86efac', borderRadius: 6, padding: '10px 16px', fontSize: 13, color: '#15803d', marginBottom: 12 },
  tabs: { display: 'flex', borderBottom: '2px solid var(--color-border)', marginBottom: 0, gap: 4 },
  tab: { padding: '10px 20px', fontSize: 13, fontWeight: 500, border: 'none', background: 'none', cursor: 'pointer', color: 'var(--color-text-muted)', borderBottom: '2px solid transparent', marginBottom: -2, display: 'flex', gap: 8, alignItems: 'center' },
  tabActive: { color: 'var(--color-primary)', borderBottomColor: 'var(--color-primary)' },
  tabBadge: { backgroundColor: '#dc2626', color: '#fff', borderRadius: 10, padding: '1px 7px', fontSize: 11 },
  card: { backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8, padding: '20px 24px', borderTopLeftRadius: 0 },
  cardTitle: { fontSize: 15, fontWeight: 600, marginBottom: 16 },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626' },
  center: { display: 'flex', justifyContent: 'center', padding: '40px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th:    { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid var(--color-border)', fontSize: 12, fontWeight: 600, color: 'var(--color-text-muted)', whiteSpace: 'nowrap', backgroundColor: 'var(--color-bg)' },
  tr:    { borderBottom: '1px solid var(--color-border)' },
  td:    { padding: '10px 12px', verticalAlign: 'middle' },
  ellipsis: { display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 160 },
  btnApprove:  { padding: '5px 14px', backgroundColor: '#15803d', color: '#fff', border: 'none', borderRadius: 4, fontSize: 12, cursor: 'pointer', whiteSpace: 'nowrap' },
  btnReject:   { padding: '5px 14px', backgroundColor: 'transparent', color: '#dc2626', border: '1px solid #dc2626', borderRadius: 4, fontSize: 12, cursor: 'pointer', whiteSpace: 'nowrap' },
  btnRejectLg: { padding: '8px 20px', backgroundColor: '#dc2626', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnSecondary:{ padding: '8px 20px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnLink:     { background: 'none', border: 'none', color: 'var(--color-primary)', textDecoration: 'underline', fontSize: 13, cursor: 'pointer', padding: '0 4px' },
  pagination:  { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, paddingTop: 16, borderTop: '1px solid var(--color-border)', marginTop: 8 },
  pageBtn: { padding: '5px 12px', border: '1px solid var(--color-border)', borderRadius: 4, backgroundColor: 'var(--color-surface)', cursor: 'pointer' },
  overlay: { position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 },
  modalBox: { backgroundColor: '#fff', borderRadius: 10, padding: '28px 32px', width: 440, maxWidth: '90vw', boxShadow: '0 8px 32px rgba(0,0,0,0.2)' },
  modalTitle: { fontSize: 16, fontWeight: 700, marginBottom: 8, margin: 0 },
  modalRef:   { fontSize: 13, color: 'var(--color-text-muted)', marginBottom: 16 },
  field:    { display: 'flex', flexDirection: 'column', gap: 4 },
  label:    { fontSize: 12, fontWeight: 500, color: 'var(--color-text-muted)' },
  textarea: { padding: '8px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, fontFamily: 'inherit', resize: 'vertical' },
}
