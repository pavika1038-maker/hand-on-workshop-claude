// SF-006: Leave Request Status Tracking (SCR-005) — รายละเอียดคำร้องเดี่ยว
import { useState, useEffect, useCallback } from 'react'
import { Link, useParams, useNavigate } from 'react-router-dom'
import { apiGet } from '../../api'
import type { ApiResponse, LeaveRequestDetail } from '../../types/leaveRequest'
import { STATUS_CONFIG, formatDate } from '../../types/leaveRequest'
import CancelLeaveModal, { type CancelTarget } from '../../components/shared/CancelLeaveModal'

interface TimelineEvent {
  eventType: string
  eventLabel: string
  actorName: string
  actionAt: string
  reason: string | null
}

const TIMELINE_ICON: Record<string, string> = {
  Created: '📝', Approved: '✅', Rejected: '❌',
  CancelRequested: '🔄', CancellationApproved: '✅', CancellationRejected: '❌', Cancelled: '🗑️',
}

function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const [datePart, timePart] = iso.split('T')
  const [y, m, d] = datePart.split('-')
  const time = (timePart ?? '').substring(0, 5)
  return `${d}/${m}/${y}${time ? ` ${time}` : ''}`
}

export default function LeaveRequestDetailPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()

  const [detail, setDetail]   = useState<LeaveRequestDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError]     = useState<string | null>(null)
  const [cancelTarget, setCancelTarget] = useState<CancelTarget | null>(null)
  const [cancelSuccess, setCancelSuccess] = useState<string | null>(null)

  // SF-013: audit trail timeline
  const [timeline, setTimeline] = useState<TimelineEvent[] | null>(null)
  const [loadingTimeline, setLoadingTimeline] = useState(false)
  const [showTimeline, setShowTimeline] = useState(false)

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true); setError(null)
    try {
      const json = await apiGet<ApiResponse<LeaveRequestDetail>>(`/api/v1/leave-requests/${id}`)
      if (json.success && json.data) setDetail(json.data)
      else setError(json.message ?? 'ไม่พบคำร้องที่ระบุ')
    } catch {
      setError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => { load() }, [load])

  const handleCancelSuccess = (message: string) => {
    setCancelTarget(null)
    setCancelSuccess(message)
    load()
    setTimeout(() => setCancelSuccess(null), 4000)
  }

  const canCancel = (status: string) => status === 'Pending' || status === 'Approved'

  const toggleTimeline = async () => {
    if (showTimeline) { setShowTimeline(false); return }
    setShowTimeline(true)
    if (timeline === null && id) {
      setLoadingTimeline(true)
      try {
        const json = await apiGet<ApiResponse<TimelineEvent[]>>(`/api/v1/leave-requests/${id}/timeline`)
        setTimeline(json.success && json.data ? json.data : [])
      } catch {
        setTimeline([])
      } finally {
        setLoadingTimeline(false)
      }
    }
  }

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <Link to="/leave-requests" style={s.crumbLink}>คำร้องขอลา</Link>
        <span style={s.crumbSep}>›</span>
        <span>รายละเอียด</span>
      </nav>

      {cancelSuccess && <div style={s.successBanner}>{cancelSuccess}</div>}

      {loading ? (
        <div style={s.center}>⏳ กำลังโหลด...</div>
      ) : error ? (
        <div style={s.errorBox}>
          {error} <button style={s.btnLink} onClick={load}>ลองใหม่</button>
        </div>
      ) : !detail ? (
        <div style={s.center}>ไม่พบคำร้อง</div>
      ) : (
        <section style={s.card}>
          {/* Header */}
          <div style={s.header}>
            <div>
              <h2 style={s.title}>{detail.leaveTypeName}</h2>
              <p style={s.ref}>{detail.leaveRequestRef}</p>
            </div>
            {(() => {
              const cfg = STATUS_CONFIG[detail.status] ?? STATUS_CONFIG.Cancelled
              return <span style={{ ...s.badge, backgroundColor: cfg.bg, color: cfg.color }}>{cfg.label}</span>
            })()}
          </div>

          {/* Fields */}
          <dl style={s.grid}>
            <Row label="ผู้ยื่นคำร้อง" value={`${detail.employeeFullNameTh} (${detail.employeeId})`} />
            <Row label="ช่วงวันที่ลา" value={
              `${formatDate(detail.startDate)}${detail.startDate !== detail.endDate ? ` – ${formatDate(detail.endDate)}` : ''}`
            } />
            <Row label="จำนวนวัน" value={`${detail.durationDays} วัน${detail.isHalfDay ? ` (ครึ่งวัน ${detail.halfDayPeriod ?? ''})` : ''}`} />
            <Row label="เหตุผลการลา" value={detail.reason || '—'} />
            <Row label="วันที่ยื่น" value={formatDateTime(detail.createdAt)} />
            {detail.approvedBy && <Row label="อนุมัติโดย" value={`${detail.approvedBy} · ${formatDateTime(detail.approvedAt)}`} />}
            {detail.rejectedBy && <Row label="ปฏิเสธโดย" value={`${detail.rejectedBy} · ${formatDateTime(detail.rejectedAt)}`} />}
            {detail.rejectionReason && <Row label="เหตุผลการปฏิเสธ" value={detail.rejectionReason} />}
          </dl>

          {/* Attachments (IF-004 — ใบรับรองแพทย์) */}
          {detail.attachments && detail.attachments.length > 0 && (
            <div style={s.attachSection}>
              <h3 style={s.attachTitle}>ใบรับรองแพทย์ / ไฟล์แนบ</h3>
              <ul style={s.attachList}>
                {detail.attachments.map(a => (
                  <li key={a.attachmentId} style={s.attachItem}>
                    <span>📎 {a.fileName}{' '}
                      <small style={{ color: 'var(--color-text-muted)' }}>({(a.fileSize / 1024).toFixed(0)} KB)</small>
                    </span>
                    <a href={`/api/v1/attachments/${a.attachmentId}`} target="_blank" rel="noreferrer" style={s.attachLink}>
                      เปิดดู
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Audit Trail Timeline (SF-013) */}
          <div style={s.timelineSection}>
            <button style={s.timelineToggle} onClick={toggleTimeline}>
              {showTimeline ? '▾' : '▸'} ประวัติการดำเนินการ
            </button>
            {showTimeline && (
              loadingTimeline ? (
                <p style={s.hint}>⏳ กำลังโหลด...</p>
              ) : !timeline || timeline.length === 0 ? (
                <p style={s.hint}>ไม่มีประวัติการดำเนินการ</p>
              ) : (
                <ul style={s.timeline}>
                  {timeline.map((ev, i) => (
                    <li key={i} style={s.timelineItem}>
                      <span style={s.timelineIcon}>{TIMELINE_ICON[ev.eventType] ?? '•'}</span>
                      <div style={s.timelineBody}>
                        <div style={s.timelineHead}>
                          <strong>{ev.eventLabel}</strong>
                          <span style={s.timelineTime}>{formatDateTime(ev.actionAt)}</span>
                        </div>
                        <div style={s.timelineActor}>โดย {ev.actorName}</div>
                        {ev.reason && <div style={s.timelineReason}>“{ev.reason}”</div>}
                      </div>
                    </li>
                  ))}
                </ul>
              )
            )}
          </div>

          {/* Actions */}
          <div style={s.actions}>
            <button style={s.btnSecondary} onClick={() => navigate('/leave-requests')}>‹ กลับ</button>
            {canCancel(detail.status) && (
              <button style={s.btnDanger} onClick={() => setCancelTarget({
                leaveRequestId: detail.leaveRequestId,
                leaveRequestRef: detail.leaveRequestRef,
                status: detail.status,
              })}>
                ยกเลิกคำร้อง
              </button>
            )}
          </div>
        </section>
      )}

      {cancelTarget && (
        <CancelLeaveModal
          target={cancelTarget}
          onClose={() => setCancelTarget(null)}
          onSuccess={handleCancelSuccess}
        />
      )}
    </div>
  )
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <>
      <dt style={s.dt}>{label}</dt>
      <dd style={s.dd}>{value}</dd>
    </>
  )
}

const s: Record<string, React.CSSProperties> = {
  breadcrumb: { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 20, fontSize: 13, flexWrap: 'wrap' },
  crumbLink:  { color: 'var(--color-primary)' },
  crumbSep:   { color: 'var(--color-text-muted)', margin: '0 4px' },
  successBanner: { backgroundColor: '#e8f5e9', border: '1px solid #86efac', borderRadius: 6, padding: '10px 16px', fontSize: 13, color: '#15803d', marginBottom: 12 },
  card: { backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8, padding: '24px 28px', maxWidth: 640 },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 12, marginBottom: 20, paddingBottom: 16, borderBottom: '1px solid var(--color-border)' },
  title: { fontSize: 18, fontWeight: 700, margin: 0, color: 'var(--color-text)' },
  ref:   { fontSize: 13, color: 'var(--color-text-muted)', margin: '4px 0 0' },
  badge: { display: 'inline-block', padding: '4px 12px', borderRadius: 12, fontSize: 12, fontWeight: 500, whiteSpace: 'nowrap' },
  grid:  { display: 'grid', gridTemplateColumns: '140px 1fr', gap: '12px 16px', margin: 0 },
  dt:    { fontSize: 13, color: 'var(--color-text-muted)', fontWeight: 500 },
  dd:    { fontSize: 13, color: 'var(--color-text)', margin: 0, lineHeight: 1.6 },
  attachSection: { marginTop: 20, paddingTop: 16, borderTop: '1px solid var(--color-border)' },
  attachTitle: { fontSize: 14, fontWeight: 600, margin: '0 0 10px', color: 'var(--color-text)' },
  attachList:  { listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: 8 },
  attachItem:  { display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12, fontSize: 13 },
  attachLink:  { color: 'var(--color-primary)', fontSize: 13, whiteSpace: 'nowrap' },
  timelineSection: { marginTop: 20, paddingTop: 16, borderTop: '1px solid var(--color-border)' },
  timelineToggle: { background: 'none', border: 'none', color: 'var(--color-primary)', fontSize: 14, fontWeight: 600, cursor: 'pointer', padding: 0 },
  timeline: { listStyle: 'none', padding: 0, margin: '14px 0 0', display: 'flex', flexDirection: 'column', gap: 14 },
  timelineItem: { display: 'flex', gap: 12, alignItems: 'flex-start' },
  timelineIcon: { fontSize: 16, lineHeight: '20px', width: 22, textAlign: 'center', flexShrink: 0 },
  timelineBody: { flex: 1, borderLeft: '2px solid var(--color-border)', paddingLeft: 12, paddingBottom: 2 },
  timelineHead: { display: 'flex', justifyContent: 'space-between', gap: 10, alignItems: 'baseline', flexWrap: 'wrap' },
  timelineTime: { fontSize: 12, color: 'var(--color-text-muted)', whiteSpace: 'nowrap' },
  timelineActor: { fontSize: 12, color: 'var(--color-text-muted)', marginTop: 2 },
  timelineReason: { fontSize: 12, color: 'var(--color-text)', marginTop: 4, fontStyle: 'italic' },
  hint: { color: 'var(--color-text-muted)', fontSize: 13, marginTop: 10 },
  actions: { display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 24, paddingTop: 16, borderTop: '1px solid var(--color-border)' },
  btnSecondary: { padding: '8px 18px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnDanger:    { padding: '8px 18px', backgroundColor: '#dc2626', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnLink:      { background: 'none', border: 'none', color: 'var(--color-primary)', textDecoration: 'underline', fontSize: 13, cursor: 'pointer', padding: '0 4px' },
  center:  { display: 'flex', justifyContent: 'center', padding: '48px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626' },
}
