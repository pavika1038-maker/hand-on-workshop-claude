// SCR-008: HR Monitoring Dashboard (SFR-011)
import { useState, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { apiGet } from '../../api'
import type { ApiResponse, PagedResult } from '../../types/leaveRequest'
import { STATUS_CONFIG, formatDate } from '../../types/leaveRequest'

interface HrLeaveRequest {
  leaveRequestId: string
  leaveRequestRef: string
  employeeId: string
  employeeFullNameTh: string
  department?: string
  leaveTypeName: string
  startDate: string
  endDate: string
  durationDays: number
  status: string
  createdAt: string
}

const PAGE_SIZE = 20

const STATUS_OPTS = [
  { value: '', label: 'ทุกสถานะ' },
  { value: 'Pending',         label: 'รอการอนุมัติ' },
  { value: 'Approved',        label: 'อนุมัติแล้ว' },
  { value: 'Rejected',        label: 'ปฏิเสธ' },
  { value: 'Cancelled',       label: 'ยกเลิกแล้ว' },
  { value: 'CancelRequested', label: 'ขอยกเลิก' },
  { value: 'Escalated',       label: 'Escalated (เกิน SLA)' },
]

export default function HrDashboardPage() {
  const [items, setItems]           = useState<HrLeaveRequest[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage]             = useState(1)
  const [loading, setLoading]       = useState(false)
  const [error, setError]           = useState<string | null>(null)
  const [searched, setSearched]     = useState(false)

  const [filterStatus, setFilterStatus]   = useState('')
  const [filterDept, setFilterDept]       = useState('')
  const [appliedStatus, setAppliedStatus] = useState('')
  const [appliedDept, setAppliedDept]     = useState('')

  const loadData = useCallback(async (pg: number, st: string, dept: string) => {
    setLoading(true); setError(null)
    try {
      const qs = new URLSearchParams({ page: String(pg), pageSize: String(PAGE_SIZE) })
      if (st)   qs.set('status', st)
      if (dept) qs.set('department', dept)
      const json = await apiGet<ApiResponse<PagedResult<HrLeaveRequest>>>(`/api/v1/hr/leave-requests?${qs}`)
      if (json.success && json.data) {
        setItems(json.data.items); setTotalCount(json.data.totalCount); setTotalPages(json.data.totalPages)
        setSearched(true)
      } else {
        setError(json.message ?? 'ไม่สามารถโหลดข้อมูลได้')
      }
    } catch { setError('เกิดข้อผิดพลาดในการเชื่อมต่อ') }
    finally  { setLoading(false) }
  }, [])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setAppliedStatus(filterStatus); setAppliedDept(filterDept); setPage(1)
    loadData(1, filterStatus, filterDept)
  }
  const handleReset = () => {
    setFilterStatus(''); setFilterDept(''); setAppliedStatus(''); setAppliedDept('')
    setPage(1); loadData(1, '', '')
  }
  const changePage = (pg: number) => { setPage(pg); loadData(pg, appliedStatus, appliedDept) }

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span>HR Dashboard</span>
      </nav>

      <section style={s.card}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <h2 style={s.cardTitle}>📊 รายการลาทั้งองค์กร</h2>
          {searched && !loading && <span style={s.countBadge}>{totalCount} รายการ</span>}
        </div>
        <form onSubmit={handleSearch} style={{ display: 'flex', gap: 10, marginBottom: 20, flexWrap: 'wrap', alignItems: 'flex-end' }}>
          <div style={s.field}>
            <label style={s.label}>สถานะ</label>
            <select style={s.select} value={filterStatus} onChange={e => setFilterStatus(e.target.value)}>
              {STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <div style={s.field}>
            <label style={s.label}>แผนก</label>
            <input type="text" style={s.input} value={filterDept}
              onChange={e => setFilterDept(e.target.value)} placeholder="เช่น Engineering, HR" />
          </div>
          <button type="submit" style={s.btnPrimary} disabled={loading}>ค้นหา</button>
          <button type="button" style={s.btnSecondary} onClick={handleReset}>รีเซ็ต</button>
        </form>

        {!searched
          ? <div style={s.center}><div style={{ textAlign: 'center' }}><div style={{ fontSize: 40 }}>📊</div><p style={{ marginTop: 8 }}>กรอกเงื่อนไขแล้วกด <strong>ค้นหา</strong></p></div></div>
          : loading ? <div style={s.center}>⏳ กำลังโหลด...</div>
          : error   ? <div style={s.errorBox}>{error}</div>
          : items.length === 0 ? <div style={s.center}>ไม่พบรายการ</div>
          : (
            <>
              <div style={{ overflowX: 'auto' }}>
                <table style={s.table}>
                  <thead><tr>
                    {['พนักงาน','แผนก','ประเภทการลา','วันที่','วัน','สถานะ','วันที่ยื่น'].map(h => <th key={h} style={s.th}>{h}</th>)}
                  </tr></thead>
                  <tbody>
                    {items.map(item => {
                      const cfg = STATUS_CONFIG[item.status] ?? STATUS_CONFIG.Cancelled
                      return (
                        <tr key={item.leaveRequestId} style={s.tr}>
                          <td style={s.td}>
                            <div style={{ fontWeight: 500 }}>{item.employeeFullNameTh}</div>
                            <div style={{ fontSize: 11, color: 'var(--color-text-muted)' }}>{item.leaveRequestRef}</div>
                          </td>
                          <td style={s.td}>{item.department ?? '—'}</td>
                          <td style={s.td}>{item.leaveTypeName}</td>
                          <td style={{ ...s.td, whiteSpace: 'nowrap' }}>
                            {formatDate(item.startDate)}{item.startDate !== item.endDate ? ` – ${formatDate(item.endDate)}` : ''}
                          </td>
                          <td style={{ ...s.td, textAlign: 'center' }}>{item.durationDays}</td>
                          <td style={s.td}>
                            <span style={{ ...s.badge, backgroundColor: cfg.bg, color: cfg.color }}>{cfg.label}</span>
                          </td>
                          <td style={{ ...s.td, whiteSpace: 'nowrap', color: 'var(--color-text-muted)' }}>{formatDate(item.createdAt)}</td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
              {totalPages > 1 && (
                <div style={s.pagination}>
                  <button style={s.pageBtn} disabled={page === 1} onClick={() => changePage(page - 1)}>‹</button>
                  <span style={{ fontSize: 13 }}>หน้า {page} / {totalPages}</span>
                  <button style={s.pageBtn} disabled={page === totalPages} onClick={() => changePage(page + 1)}>›</button>
                </div>
              )}
            </>
          )
        }
      </section>
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  breadcrumb: { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 20, fontSize: 13 },
  crumbLink:  { color: 'var(--color-primary)' }, crumbSep: { color: 'var(--color-text-muted)', margin: '0 4px' },
  card: { backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8, padding: '20px 24px' },
  cardTitle: { fontSize: 15, fontWeight: 600, margin: 0 },
  countBadge: { fontSize: 12, backgroundColor: 'var(--color-primary-light)', color: 'var(--color-primary)', borderRadius: 10, padding: '2px 10px' },
  field: { display: 'flex', flexDirection: 'column', gap: 4 },
  label: { fontSize: 12, fontWeight: 500, color: 'var(--color-text-muted)' },
  select: { padding: '7px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, backgroundColor: 'var(--color-surface)' },
  input:  { padding: '7px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, minWidth: 180 },
  btnPrimary:  { padding: '7px 20px', backgroundColor: 'var(--color-primary)', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  btnSecondary:{ padding: '7px 20px', backgroundColor: 'transparent', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, cursor: 'pointer' },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626' },
  center: { display: 'flex', justifyContent: 'center', alignItems: 'center', padding: '60px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th:    { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid var(--color-border)', fontSize: 12, fontWeight: 600, color: 'var(--color-text-muted)', whiteSpace: 'nowrap', backgroundColor: 'var(--color-bg)' },
  tr:    { borderBottom: '1px solid var(--color-border)' },
  td:    { padding: '10px 12px', verticalAlign: 'middle' },
  badge: { display: 'inline-block', padding: '3px 10px', borderRadius: 10, fontSize: 12, fontWeight: 500, whiteSpace: 'nowrap' },
  pagination: { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, paddingTop: 16, borderTop: '1px solid var(--color-border)', marginTop: 8 },
  pageBtn: { padding: '5px 12px', border: '1px solid var(--color-border)', borderRadius: 4, backgroundColor: 'var(--color-surface)', cursor: 'pointer' },
}
