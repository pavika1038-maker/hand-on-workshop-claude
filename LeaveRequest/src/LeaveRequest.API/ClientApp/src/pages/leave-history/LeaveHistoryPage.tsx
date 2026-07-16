// SCR-005/006: ประวัติการลา + Balance Summary (SFR-006, SFR-002)
import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { apiGet } from '../../api'
import type { ApiResponse, LeaveBalanceDashboard, LeaveRequestSummary, PagedResult } from '../../types/leaveRequest'
import { STATUS_CONFIG, formatDate } from '../../types/leaveRequest'

const PAGE_SIZE = 15

const STATUS_OPTS = [
  { value: '',                label: 'ทุกสถานะ' },
  { value: 'Pending',         label: 'รอการอนุมัติ' },
  { value: 'Approved',        label: 'อนุมัติแล้ว' },
  { value: 'Rejected',        label: 'ปฏิเสธ' },
  { value: 'Cancelled',       label: 'ยกเลิกแล้ว' },
  { value: 'CancelRequested', label: 'ขอยกเลิก' },
  { value: 'Escalated',       label: 'Escalated (เกิน SLA)' },
]

function balanceColor(rem: number, entitled: number) {
  if (entitled === 0) return '#6b7280'
  const pct = rem / entitled
  return pct < 0.25 ? '#dc2626' : pct < 0.5 ? '#d97706' : '#15803d'
}

export default function LeaveHistoryPage() {
  const { user } = useAuth()
  const employeeId = user?.employeeId ?? ''

  const [dashboard, setDashboard]         = useState<LeaveBalanceDashboard | null>(null)
  const [loadingBal, setLoadingBal]       = useState(true)
  const [balError, setBalError]           = useState<string | null>(null)

  const [items, setItems]                 = useState<LeaveRequestSummary[]>([])
  const [totalCount, setTotalCount]       = useState(0)
  const [totalPages, setTotalPages]       = useState(1)
  const [page, setPage]                   = useState(1)
  const [loadingHistory, setLoadingHistory] = useState(true)
  const [historyError, setHistoryError]   = useState<string | null>(null)
  const [filterStatus, setFilterStatus]   = useState('')
  const [appliedStatus, setAppliedStatus] = useState('')
  const [refreshVer, setRefreshVer]       = useState(0)

  useEffect(() => {
    if (!employeeId) return
    let active = true
    setLoadingBal(true)
    apiGet<ApiResponse<LeaveBalanceDashboard>>(`/api/v1/leave-balances/dashboard?employeeId=${employeeId}`)
      .then(json => {
        if (!active) return
        if (json.success && json.data) setDashboard(json.data)
        else setBalError(json.message ?? 'ไม่สามารถโหลดข้อมูลวันลาได้')
      })
      .catch(() => { if (active) setBalError('เกิดข้อผิดพลาดในการเชื่อมต่อ') })
      .finally(() => { if (active) setLoadingBal(false) })
    return () => { active = false }
  }, [employeeId])

  const loadHistory = useCallback(async () => {
    if (!employeeId) return
    setLoadingHistory(true); setHistoryError(null)
    try {
      const qs = new URLSearchParams({ employeeId, page: String(page), pageSize: String(PAGE_SIZE) })
      if (appliedStatus) qs.set('status', appliedStatus)
      const json = await apiGet<ApiResponse<PagedResult<LeaveRequestSummary>>>(`/api/v1/leave-requests?${qs}`)
      if (json.success && json.data) {
        setItems(json.data.items); setTotalCount(json.data.totalCount); setTotalPages(json.data.totalPages)
      } else {
        setHistoryError(json.message ?? 'ไม่สามารถโหลดประวัติได้')
      }
    } catch { setHistoryError('เกิดข้อผิดพลาดในการเชื่อมต่อ') }
    finally  { setLoadingHistory(false) }
  }, [employeeId, page, appliedStatus, refreshVer])

  useEffect(() => { loadHistory() }, [loadHistory])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault(); setAppliedStatus(filterStatus); setPage(1); setRefreshVer(v => v + 1)
  }
  const handleReset = () => {
    setFilterStatus(''); setAppliedStatus(''); setPage(1); setRefreshVer(v => v + 1)
  }

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span>ประวัติการลา</span>
      </nav>

      {/* Balance Cards */}
      <section style={s.card}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 14 }}>
          <h2 style={{ ...s.cardTitle, margin: 0 }}>วันลาคงเหลือ</h2>
          {dashboard && <span style={s.yearBadge}>ปี {dashboard.leaveYear}</span>}
        </div>
        {loadingBal ? <p style={s.hint}>⏳ กำลังโหลด...</p>
        : balError   ? <div style={s.errorBox}>{balError}</div>
        : !dashboard?.balances.length ? <p style={s.hint}>ยังไม่มีข้อมูลสิทธิ์วันลา</p>
        : (
          <div style={s.balGrid}>
            {dashboard.balances.map(b => {
              const color = balanceColor(b.remainingDays, b.entitledDays)
              const pct = b.entitledDays > 0 ? Math.min(100, (b.remainingDays / b.entitledDays) * 100) : 0
              return (
                <div key={b.leaveTypeId} style={s.balCard}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                    <span style={{ fontSize: 13, fontWeight: 500 }}>{b.typeNameTh}</span>
                    <span style={{ fontSize: 20, fontWeight: 700, color }}>{b.remainingDays}</span>
                  </div>
                  <div style={s.barTrack}>
                    <div style={{ ...s.barFill, width: `${pct}%`, backgroundColor: color }} />
                  </div>
                  <div style={s.balDetail}>
                    <span>สิทธิ์ {b.entitledDays}</span>
                    <span>ใช้ {b.usedDays} · รอ {b.pendingDays}</span>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </section>

      {/* History */}
      <section style={{ ...s.card, marginTop: 20 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14 }}>
          <h2 style={{ ...s.cardTitle, margin: 0 }}>ประวัติการลา</h2>
          {!loadingHistory && <span style={s.countBadge}>{totalCount} รายการ</span>}
        </div>

        {/* Filter */}
        <form onSubmit={handleSearch} style={{ display: 'flex', gap: 10, marginBottom: 16, flexWrap: 'wrap' }}>
          <select style={s.select} value={filterStatus} onChange={e => setFilterStatus(e.target.value)}>
            {STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          <button type="submit" style={s.btnPrimary}>ค้นหา</button>
          <button type="button" style={s.btnSecondary} onClick={handleReset}>รีเซ็ต</button>
        </form>

        {loadingHistory ? <div style={s.center}>⏳ กำลังโหลด...</div>
        : historyError   ? <div style={s.errorBox}>{historyError}</div>
        : items.length === 0 ? <div style={s.center}>📅 ไม่พบรายการ</div>
        : (
          <div style={{ overflowX: 'auto' }}>
            <table style={s.table}>
              <thead><tr>
                {['ประเภทการลา','วันที่','วัน','เหตุผล','สถานะ','วันที่ยื่น'].map(h => (
                  <th key={h} style={s.th}>{h}</th>
                ))}
              </tr></thead>
              <tbody>
                {items.map(item => {
                  const cfg = STATUS_CONFIG[item.status] ?? STATUS_CONFIG.Cancelled
                  return (
                    <tr key={item.leaveRequestId} style={s.tr}>
                      <td style={s.td}>{item.leaveTypeName}</td>
                      <td style={{ ...s.td, whiteSpace: 'nowrap' }}>
                        {formatDate(item.startDate)}{item.startDate !== item.endDate ? ` – ${formatDate(item.endDate)}` : ''}
                      </td>
                      <td style={{ ...s.td, textAlign: 'center' }}>{item.durationDays}</td>
                      <td style={{ ...s.td, maxWidth: 180 }}>
                        <span style={s.ellipsis}>{item.reason ?? '—'}</span>
                      </td>
                      <td style={s.td}>
                        <span style={{ ...s.badge, backgroundColor: cfg.bg, color: cfg.color }}>{cfg.label}</span>
                      </td>
                      <td style={{ ...s.td, whiteSpace: 'nowrap', color: 'var(--color-text-muted)' }}>
                        {formatDate(item.createdAt)}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}

        {!loadingHistory && totalPages > 1 && (
          <div style={s.pagination}>
            <button style={s.pageBtn} disabled={page === 1} onClick={() => setPage(p => p - 1)}>‹</button>
            <span style={{ fontSize: 13 }}>หน้า {page} / {totalPages}</span>
            <button style={s.pageBtn} disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>›</button>
          </div>
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
  cardTitle: { fontSize: 15, fontWeight: 600, margin: 0 },
  yearBadge: { fontSize: 11, backgroundColor: 'var(--color-primary-light)', color: 'var(--color-primary)', borderRadius: 10, padding: '2px 10px' },
  countBadge: { fontSize: 12, backgroundColor: 'var(--color-primary-light)', color: 'var(--color-primary)', borderRadius: 10, padding: '2px 10px' },
  hint: { color: 'var(--color-text-muted)', fontSize: 13 },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626' },
  balGrid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 12 },
  balCard: { backgroundColor: 'var(--color-bg)', border: '1px solid var(--color-border)', borderRadius: 6, padding: '12px 14px' },
  barTrack: { height: 5, borderRadius: 3, backgroundColor: '#e5e7eb', overflow: 'hidden', margin: '6px 0' },
  barFill:  { height: '100%', borderRadius: 3 },
  balDetail: { display: 'flex', justifyContent: 'space-between', fontSize: 11, color: 'var(--color-text-muted)' },
  select: { padding: '7px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, backgroundColor: 'var(--color-surface)' },
  btnPrimary:  { padding: '7px 18px', backgroundColor: 'var(--color-primary)', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnSecondary:{ padding: '7px 18px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  center: { display: 'flex', justifyContent: 'center', padding: '40px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th:    { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid var(--color-border)', fontSize: 12, fontWeight: 600, color: 'var(--color-text-muted)', whiteSpace: 'nowrap', backgroundColor: 'var(--color-bg)' },
  tr:    { borderBottom: '1px solid var(--color-border)' },
  td:    { padding: '10px 12px', verticalAlign: 'middle' },
  badge: { display: 'inline-block', padding: '3px 10px', borderRadius: 10, fontSize: 12, fontWeight: 500, whiteSpace: 'nowrap' },
  ellipsis: { display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 180 },
  pagination: { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, paddingTop: 16, borderTop: '1px solid var(--color-border)', marginTop: 8 },
  pageBtn: { padding: '5px 12px', border: '1px solid var(--color-border)', borderRadius: 4, backgroundColor: 'var(--color-surface)', cursor: 'pointer' },
}
