import { useState } from 'react'
import { apiGet } from '../../api'
import { formatDate, STATUS_CONFIG } from '../../types/leaveRequest'

interface LeaveHistoryItem {
  leaveRequestId: string
  leaveRequestRef: string
  employeeId: string
  employeeFullNameTh: string
  department: string | null
  leaveTypeNameTh: string
  startDate: string
  endDate: string
  durationDays: number
  isHalfDay: boolean
  status: string
  approvedBy: string | null
  rejectedBy: string | null
  rejectionReason: string | null
  createdAt: string
}

interface ApiResponse<T> {
  success: boolean
  data: T
  errorCode?: string
  message?: string
  metadata?: { page: number; pageSize: number; totalCount: number; totalPages: number }
}

interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

const LEAVE_TYPE_OPTIONS = [
  { value: '', label: 'ทุกประเภท' },
  { value: '1', label: 'ลาพักร้อน' },
  { value: '2', label: 'ลาป่วย' },
  { value: '3', label: 'ลากิจ' },
  { value: '4', label: 'ลาคลอด' },
]

const STATUS_OPTIONS = [
  { value: '', label: 'ทุก Status' },
  { value: 'Pending', label: 'รอการอนุมัติ' },
  { value: 'Approved', label: 'อนุมัติแล้ว' },
  { value: 'Rejected', label: 'ปฏิเสธ' },
  { value: 'Cancelled', label: 'ยกเลิกแล้ว' },
]

export default function LeaveReportPage() {
  const [startDate, setStartDate]     = useState('')
  const [endDate, setEndDate]         = useState('')
  const [employeeId, setEmployeeId]   = useState('')
  const [leaveTypeId, setLeaveTypeId] = useState('')
  const [status, setStatus]           = useState('')
  const [department, setDepartment]   = useState('')
  const [page, setPage]               = useState(1)

  const [items, setItems]       = useState<LeaveHistoryItem[]>([])
  const [totalCount, setTotal]  = useState(0)
  const [totalPages, setTotalPages] = useState(0)
  const [loading, setLoading]   = useState(false)
  const [error, setError]       = useState('')
  const [searched, setSearched] = useState(false)

  const PAGE_SIZE = 20

  const buildQuery = (p: number) => {
    const params = new URLSearchParams()
    if (startDate)   params.set('startDate',   startDate)
    if (endDate)     params.set('endDate',      endDate)
    if (employeeId)  params.set('employeeId',   employeeId.trim())
    if (leaveTypeId) params.set('leaveTypeId',  leaveTypeId)
    if (status)      params.set('status',       status)
    if (department)  params.set('department',   department.trim())
    params.set('page',     String(p))
    params.set('pageSize', String(PAGE_SIZE))
    return params.toString()
  }

  const search = async (p = 1) => {
    setLoading(true)
    setError('')
    setSearched(true)
    try {
      const json = await apiGet<ApiResponse<PagedResult<LeaveHistoryItem>>>(
        `/api/v1/reports/leave-history?${buildQuery(p)}`
      )
      if (!json.success) { setError(json.message ?? 'เกิดข้อผิดพลาด'); return }
      setItems(json.data.items)
      setTotal(json.data.totalCount)
      setTotalPages(json.data.totalPages)
      setPage(p)
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'ไม่สามารถโหลดข้อมูลได้')
    } finally {
      setLoading(false)
    }
  }

  const handleExportCsv = () => {
    if (items.length === 0) return
    const headers = ['Ref', 'รหัสพนักงาน', 'ชื่อ', 'แผนก', 'ประเภทการลา', 'วันเริ่ม', 'วันสิ้นสุด', 'จำนวนวัน', 'Status']
    const rows = items.map(r => [
      r.leaveRequestRef,
      r.employeeId,
      r.employeeFullNameTh,
      r.department ?? '',
      r.leaveTypeNameTh,
      r.startDate,
      r.endDate,
      r.durationDays,
      r.status,
    ].map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))
    const csv = [headers.join(','), ...rows].join('\n')
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' })
    const url  = URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = `leave-report-${new Date().toISOString().slice(0,10)}.csv`
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div>
      <div style={s.header}>
        <h2 style={s.title}>📊 รายงานการลา</h2>
        <p style={s.sub}>รายงานประวัติการลาทั้งองค์กร (HR เท่านั้น)</p>
      </div>

      {/* ── Filter panel ── */}
      <div style={s.filterCard}>
        <div style={s.filterGrid}>
          <div style={s.field}>
            <label style={s.label}>วันที่เริ่มลา (ตั้งแต่)</label>
            <input style={s.input} type="date" value={startDate} onChange={e => setStartDate(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>วันที่เริ่มลา (ถึง)</label>
            <input style={s.input} type="date" value={endDate} onChange={e => setEndDate(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>รหัสพนักงาน</label>
            <input style={s.input} type="text" placeholder="เช่น EMP001" value={employeeId} onChange={e => setEmployeeId(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>แผนก</label>
            <input style={s.input} type="text" placeholder="เช่น IT" value={department} onChange={e => setDepartment(e.target.value)} />
          </div>
          <div style={s.field}>
            <label style={s.label}>ประเภทการลา</label>
            <select style={s.input} value={leaveTypeId} onChange={e => setLeaveTypeId(e.target.value)}>
              {LEAVE_TYPE_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <div style={s.field}>
            <label style={s.label}>Status</label>
            <select style={s.input} value={status} onChange={e => setStatus(e.target.value)}>
              {STATUS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
        </div>
        <div style={s.filterActions}>
          <button style={s.btnSearch} onClick={() => search(1)} disabled={loading}>
            {loading ? '⏳ กำลังค้นหา...' : '🔍 ค้นหา'}
          </button>
          <button style={s.btnExport} onClick={handleExportCsv} disabled={items.length === 0}>
            ⬇️ Export CSV
          </button>
        </div>
      </div>

      {/* ── Error ── */}
      {error && <div style={s.errorBox}>{error}</div>}

      {/* ── Placeholder ── */}
      {!searched && !loading && (
        <div style={s.placeholder}>กรอกเงื่อนไขและกด "ค้นหา" เพื่อดูรายงาน</div>
      )}

      {/* ── Result ── */}
      {searched && !loading && (
        <>
          <div style={s.resultMeta}>
            พบทั้งหมด <strong>{totalCount.toLocaleString()}</strong> รายการ
            {totalCount > 0 && ` (หน้า ${page} / ${totalPages})`}
          </div>

          {items.length === 0 ? (
            <div style={s.placeholder}>ไม่พบข้อมูลตามเงื่อนไขที่ค้นหา</div>
          ) : (
            <div style={s.tableWrap}>
              <table style={s.table}>
                <thead>
                  <tr>
                    {['Ref', 'พนักงาน', 'แผนก', 'ประเภทการลา', 'ช่วงลา', 'วัน', 'Status', 'วันที่ยื่น'].map(h => (
                      <th key={h} style={s.th}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {items.map(r => {
                    const cfg = STATUS_CONFIG[r.status as keyof typeof STATUS_CONFIG]
                    return (
                      <tr key={r.leaveRequestId} style={s.tr}>
                        <td style={s.td}><code style={{ fontSize: 11 }}>{r.leaveRequestRef}</code></td>
                        <td style={s.td}>
                          <div style={{ fontWeight: 600, fontSize: 13 }}>{r.employeeFullNameTh}</div>
                          <div style={{ fontSize: 11, color: '#888' }}>{r.employeeId}</div>
                        </td>
                        <td style={s.td}>{r.department ?? '—'}</td>
                        <td style={s.td}>{r.leaveTypeNameTh}</td>
                        <td style={s.td}>
                          <div style={{ fontSize: 12 }}>{r.startDate}</div>
                          <div style={{ fontSize: 11, color: '#888' }}>→ {r.endDate}</div>
                        </td>
                        <td style={{ ...s.td, textAlign: 'center' }}>
                          {r.isHalfDay ? '0.5' : r.durationDays}
                        </td>
                        <td style={s.td}>
                          {cfg ? (
                            <span style={{ ...s.badge, background: cfg.bg, color: cfg.color }}>
                              {cfg.label}
                            </span>
                          ) : r.status}
                        </td>
                        <td style={{ ...s.td, fontSize: 12 }}>{formatDate(r.createdAt)}</td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div style={s.pagination}>
              <button style={s.pageBtn} onClick={() => search(page - 1)} disabled={page <= 1 || loading}>‹ ก่อนหน้า</button>
              {Array.from({ length: totalPages }, (_, i) => i + 1)
                .filter(p => Math.abs(p - page) <= 2)
                .map(p => (
                  <button
                    key={p}
                    style={{ ...s.pageBtn, ...(p === page ? s.pageBtnActive : {}) }}
                    onClick={() => search(p)}
                    disabled={loading}
                  >{p}</button>
                ))}
              <button style={s.pageBtn} onClick={() => search(page + 1)} disabled={page >= totalPages || loading}>ถัดไป ›</button>
            </div>
          )}
        </>
      )}
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  header:       { marginBottom: 20 },
  title:        { fontSize: 22, fontWeight: 700, margin: '0 0 4px 0' },
  sub:          { fontSize: 13, color: '#666', margin: 0 },
  filterCard:   { background: '#fff', border: '1px solid #e5e7eb', borderRadius: 8, padding: 20, marginBottom: 20 },
  filterGrid:   { display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '12px 16px', marginBottom: 16 },
  field:        { display: 'flex', flexDirection: 'column', gap: 4 },
  label:        { fontSize: 12, fontWeight: 600, color: '#555' },
  input:        { padding: '8px 10px', border: '1px solid #d1d5db', borderRadius: 6, fontSize: 13, outline: 'none' },
  filterActions:{ display: 'flex', gap: 10 },
  btnSearch:    { padding: '9px 20px', background: '#0078d4', color: '#fff', border: 'none', borderRadius: 6, fontSize: 13, fontWeight: 600, cursor: 'pointer' },
  btnExport:    { padding: '9px 20px', background: '#16a34a', color: '#fff', border: 'none', borderRadius: 6, fontSize: 13, fontWeight: 600, cursor: 'pointer' },
  errorBox:     { background: '#fef2f2', border: '1px solid #fca5a5', borderRadius: 6, padding: '10px 14px', color: '#b91c1c', fontSize: 13, marginBottom: 16 },
  placeholder:  { textAlign: 'center', padding: '60px 0', color: '#9ca3af', fontSize: 14 },
  resultMeta:   { fontSize: 13, color: '#666', marginBottom: 12 },
  tableWrap:    { overflowX: 'auto', background: '#fff', border: '1px solid #e5e7eb', borderRadius: 8 },
  table:        { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th:           { padding: '10px 12px', background: '#f9fafb', borderBottom: '1px solid #e5e7eb', textAlign: 'left', fontSize: 12, fontWeight: 600, color: '#555', whiteSpace: 'nowrap' },
  tr:           { borderBottom: '1px solid #f3f4f6' },
  td:           { padding: '10px 12px', verticalAlign: 'top' },
  badge:        { display: 'inline-block', padding: '2px 8px', borderRadius: 12, fontSize: 11, fontWeight: 600 },
  pagination:   { display: 'flex', gap: 6, justifyContent: 'center', marginTop: 20 },
  pageBtn:      { padding: '6px 12px', border: '1px solid #d1d5db', borderRadius: 6, background: '#fff', fontSize: 13, cursor: 'pointer' },
  pageBtnActive:{ background: '#0078d4', color: '#fff', borderColor: '#0078d4', fontWeight: 600 },
}
