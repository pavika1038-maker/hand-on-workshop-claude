// SF-015: Notification Log View (SCR-011) — HR ตรวจสอบ log การส่ง Email (RFR-003, NFR-007)
import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { apiGet } from '../../api'
import type { ApiResponse, PagedResult } from '../../types/leaveRequest'
import { formatDate } from '../../types/leaveRequest'

interface NotificationLogItem {
  notificationLogId: string
  sentAt: string | null
  createdAt: string
  eventType: string
  requestRef: string | null
  leaveRequestId: string | null
  employeeName: string | null
  recipients: string
  status: string
  retryCount: number
  failureReason: string | null
}

interface NotificationLogReport {
  totalCount: number
  successCount: number
  failedCount: number
  successRatePct: number
  items: PagedResult<NotificationLogItem>
}

const PAGE_SIZE = 20

const EVENT_OPTS = [
  { value: '', label: 'ทุก Event' },
  { value: 'LeaveSubmitted', label: 'ยื่นคำขอลา' },
  { value: 'LeaveApproved', label: 'อนุมัติการลา' },
  { value: 'LeaveRejected', label: 'ปฏิเสธการลา' },
  { value: 'CancelRequested', label: 'ขอยกเลิก' },
  { value: 'CancellationApproved', label: 'อนุมัติการยกเลิก' },
  { value: 'CancellationRejected', label: 'ปฏิเสธการยกเลิก' },
]

const STATUS_OPTS = [
  { value: '', label: 'ทุกสถานะ' },
  { value: 'Success', label: 'สำเร็จ' },
  { value: 'Failed', label: 'ล้มเหลว' },
  { value: 'Pending', label: 'รอส่ง/Retry' },
]

const STATUS_BADGE: Record<string, { label: string; bg: string; color: string }> = {
  Success: { label: 'สำเร็จ', bg: '#e8f5e9', color: '#15803d' },
  Failed:  { label: 'ล้มเหลว', bg: '#fde8e8', color: '#dc2626' },
  Pending: { label: 'รอส่ง', bg: '#fff4e5', color: '#d97706' },
}

function today(): string {
  return new Date().toISOString().slice(0, 10)
}
function formatDateTime(iso: string | null): string {
  if (!iso) return '—'
  const [d, t] = iso.split('T')
  return `${formatDate(d)}${t ? ` ${t.substring(0, 5)}` : ''}`
}

export default function NotificationLogPage() {
  const [dateFrom, setDateFrom] = useState(today())
  const [dateTo, setDateTo]     = useState(today())
  const [eventType, setEventType] = useState('')
  const [recipient, setRecipient] = useState('')
  const [status, setStatus]     = useState('')
  const [page, setPage]         = useState(1)

  const [report, setReport]   = useState<NotificationLogReport | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError]     = useState<string | null>(null)

  const load = useCallback(async (pg: number) => {
    if (dateFrom > dateTo) { setError('วันที่เริ่มต้นต้องน้อยกว่าหรือเท่ากับวันที่สิ้นสุด'); return }
    setLoading(true); setError(null)
    try {
      const qs = new URLSearchParams({ dateFrom, dateTo, page: String(pg), pageSize: String(PAGE_SIZE) })
      if (eventType) qs.set('eventType', eventType)
      if (recipient) qs.set('recipient', recipient.trim())
      if (status)    qs.set('status', status)
      const json = await apiGet<ApiResponse<NotificationLogReport>>(`/api/v1/reports/notification-log?${qs}`)
      if (json.success && json.data) { setReport(json.data); setPage(pg) }
      else setError(json.message ?? 'ไม่สามารถโหลด log ได้')
    } catch { setError('เกิดข้อผิดพลาดในการเชื่อมต่อ') }
    finally  { setLoading(false) }
  }, [dateFrom, dateTo, eventType, recipient, status])

  // onLoad: auto-generate ด้วย default filter (วันนี้) — monitoring screen (design §7.1)
  useEffect(() => { load(1) }, []) // eslint-disable-line react-hooks/exhaustive-deps

  const totalPages = report ? Math.max(1, Math.ceil(report.items.totalCount / PAGE_SIZE)) : 1
  const rateLow = report != null && report.totalCount > 0 && report.successRatePct < 99

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span>Log การแจ้งเตือน</span>
      </nav>

      <section style={s.card}>
        <h2 style={s.cardTitle}>📧 Log การแจ้งเตือน (Notification Log)</h2>

        {/* Filter */}
        <form onSubmit={e => { e.preventDefault(); load(1) }} style={s.filterRow}>
          <div style={s.field}>
            <label style={s.label}>วันที่เริ่มต้น</label>
            <input type="date" style={s.input} value={dateFrom} onChange={e => setDateFrom(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>วันที่สิ้นสุด</label>
            <input type="date" style={s.input} value={dateTo} min={dateFrom} onChange={e => setDateTo(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>Event</label>
            <select style={s.input} value={eventType} onChange={e => setEventType(e.target.value)}>
              {EVENT_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <div style={s.field}>
            <label style={s.label}>ผู้รับ (email)</label>
            <input type="text" style={s.input} value={recipient} placeholder="เช่น hr@abc.com" onChange={e => setRecipient(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>สถานะ</label>
            <select style={s.input} value={status} onChange={e => setStatus(e.target.value)}>
              {STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <button type="submit" style={s.btnPrimary} disabled={loading}>ดูรายงาน</button>
          <button type="button" style={s.btnSecondary} disabled={loading} onClick={() => load(page)}>Refresh</button>
        </form>

        {/* Summary Strip */}
        {report && (
          <div style={{ ...s.summary, ...(rateLow ? s.summaryWarn : {}) }}>
            <span>ทั้งหมด <strong>{report.totalCount}</strong></span>
            <span>สำเร็จ <strong style={{ color: '#15803d' }}>{report.successCount}</strong></span>
            <span>ล้มเหลว <strong style={{ color: '#dc2626' }}>{report.failedCount}</strong></span>
            <span>Success Rate <strong style={{ color: rateLow ? '#dc2626' : '#15803d' }}>{report.successRatePct}%</strong></span>
            {rateLow
              ? <span style={s.kpiWarn}>⚠️ ต่ำกว่า KPI 99% — กรุณาตรวจสอบ</span>
              : report.totalCount > 0 && <span style={s.kpiOk}>✓ เป็นไปตาม KPI</span>}
          </div>
        )}

        {error && <div style={s.errorBox}>{error}</div>}

        {loading ? <div style={s.center}>⏳ กำลังโหลด...</div>
        : !report ? null
        : report.items.items.length === 0 ? <div style={s.center}>ไม่พบรายการ log ตามเงื่อนไข</div>
        : (
          <>
            <div style={{ overflowX: 'auto' }}>
              <table style={s.table}>
                <thead><tr>
                  {['วัน-เวลา', 'Event', 'อ้างอิงคำขอ', 'พนักงาน', 'ผู้รับ', 'สถานะ', 'Retry', 'สาเหตุ'].map(h =>
                    <th key={h} style={s.th}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {report.items.items.map(it => {
                    const badge = STATUS_BADGE[it.status] ?? STATUS_BADGE.Pending
                    return (
                      <tr key={it.notificationLogId} style={s.tr}>
                        <td style={{ ...s.td, whiteSpace: 'nowrap' }}>{formatDateTime(it.sentAt ?? it.createdAt)}</td>
                        <td style={s.td}>{it.eventType}</td>
                        <td style={s.td}>
                          {it.requestRef
                            ? (it.leaveRequestId
                                ? <Link to={`/leave-requests/${it.leaveRequestId}`} style={s.rowLink}>{it.requestRef}</Link>
                                : it.requestRef)
                            : '—'}
                        </td>
                        <td style={s.td}>{it.employeeName ?? '—'}</td>
                        <td style={{ ...s.td, maxWidth: 200 }}><span style={s.ellipsis} title={it.recipients}>{it.recipients || '—'}</span></td>
                        <td style={s.td}><span style={{ ...s.badge, backgroundColor: badge.bg, color: badge.color }}>{badge.label}</span></td>
                        <td style={{ ...s.td, textAlign: 'center' }}>{it.retryCount}</td>
                        <td style={{ ...s.td, maxWidth: 200, color: '#dc2626' }}>
                          <span style={s.ellipsis} title={it.failureReason ?? ''}>{it.failureReason ?? '—'}</span>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
            {totalPages > 1 && (
              <div style={s.pagination}>
                <button style={s.pageBtn} disabled={page === 1} onClick={() => load(page - 1)}>‹</button>
                <span style={{ fontSize: 13 }}>หน้า {page} / {totalPages}</span>
                <button style={s.pageBtn} disabled={page >= totalPages} onClick={() => load(page + 1)}>›</button>
              </div>
            )}
          </>
        )}
      </section>
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  breadcrumb: { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 20, fontSize: 13 },
  crumbLink:  { color: 'var(--color-primary)' },
  crumbSep:   { color: 'var(--color-text-muted)', margin: '0 4px' },
  card: { backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8, padding: '20px 24px' },
  cardTitle: { fontSize: 15, fontWeight: 600, margin: '0 0 16px' },
  filterRow: { display: 'flex', gap: 10, flexWrap: 'wrap', alignItems: 'flex-end', marginBottom: 16 },
  field: { display: 'flex', flexDirection: 'column', gap: 4 },
  label: { fontSize: 12, fontWeight: 500, color: 'var(--color-text-muted)' },
  input: { padding: '7px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, backgroundColor: 'var(--color-surface)' },
  btnPrimary:  { padding: '7px 18px', backgroundColor: 'var(--color-primary)', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnSecondary:{ padding: '7px 18px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  summary: { display: 'flex', gap: 20, flexWrap: 'wrap', alignItems: 'center', backgroundColor: 'var(--color-bg)', border: '1px solid var(--color-border)', borderRadius: 6, padding: '10px 16px', fontSize: 13, marginBottom: 16 },
  summaryWarn: { borderColor: '#fca5a5', backgroundColor: '#fffbeb' },
  kpiWarn: { marginLeft: 'auto', color: '#b45309', fontSize: 12 },
  kpiOk:   { marginLeft: 'auto', color: '#15803d', fontSize: 12 },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626', marginBottom: 12 },
  center: { display: 'flex', justifyContent: 'center', padding: '40px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th: { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid var(--color-border)', fontSize: 12, fontWeight: 600, color: 'var(--color-text-muted)', whiteSpace: 'nowrap', backgroundColor: 'var(--color-bg)' },
  tr: { borderBottom: '1px solid var(--color-border)' },
  td: { padding: '10px 12px', verticalAlign: 'middle' },
  rowLink: { color: 'var(--color-primary)', textDecoration: 'none' },
  badge: { display: 'inline-block', padding: '3px 10px', borderRadius: 10, fontSize: 12, fontWeight: 500, whiteSpace: 'nowrap' },
  ellipsis: { display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
  pagination: { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, paddingTop: 16, borderTop: '1px solid var(--color-border)', marginTop: 8 },
  pageBtn: { padding: '5px 12px', border: '1px solid var(--color-border)', borderRadius: 4, backgroundColor: 'var(--color-surface)', cursor: 'pointer' },
}
