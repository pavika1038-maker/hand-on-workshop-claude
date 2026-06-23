// SFR-015 (Leave Types read-only), SFR-012 (Import History + Upload, IF-003)
// TODO: Replace role guard with real MSAL role check when auth is configured (NFR-005)

import { useState, useEffect, useCallback, useRef } from 'react'
import { Link } from 'react-router-dom'
import { apiUploadFile } from '../../api'
import type { ApiResponse, PagedResult } from '../../types/leaveRequest'

// ── Types ─────────────────────────────────────────────────────────────────────

interface LeaveTypeItem {
  leaveTypeId: number
  leaveTypeName: string
  maxDaysPerYear?: number
  isAvailableForOutsource?: boolean
  isActive?: boolean
}

interface ImportLogItem {
  importLogId: string
  fileName: string
  importedBy: string
  totalRecords: number
  successRecords: number
  failedRecords: number
  isRolledBack: boolean
  createdAt: string
}

interface ImportErrorDetail {
  rowNumber: number
  field: string
  message: string
}

interface ImportResultDto {
  importLogId: string
  totalRecords: number
  successRecords: number
  failedRecords: number
  isRolledBack?: boolean
  errors: ImportErrorDetail[]
}

type ActiveTab = 'leave-types' | 'import'

// ── Constants ─────────────────────────────────────────────────────────────────

const PAGE_SIZE = 20

// ── Helpers ───────────────────────────────────────────────────────────────────

function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const [datePart, timePart] = iso.split('T')
  const [y, m, d] = datePart.split('-')
  const time = (timePart ?? '').substring(0, 5)
  return `${d}/${m}/${y}${time ? ` ${time}` : ''}`
}

function importBadge(log: ImportLogItem): { label: string; bg: string; color: string } {
  if (log.isRolledBack)      return { label: 'ยกเลิกทั้งหมด', bg: '#fde8e8', color: 'var(--color-error)'   }
  if (log.failedRecords > 0) return { label: 'มีข้อผิดพลาด',  bg: '#fff4e5', color: 'var(--color-warning)' }
  return                            { label: 'สำเร็จ',          bg: '#e8f5e9', color: 'var(--color-success)' }
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState<ActiveTab>('leave-types')

  // ── Tab 1: Leave Types ────────────────────────────────────────────────────
  const [leaveTypes, setLeaveTypes]       = useState<LeaveTypeItem[]>([])
  const [loadingTypes, setLoadingTypes]   = useState(true)
  const [typesError, setTypesError]       = useState<string | null>(null)

  // ── Tab 2: Upload ──────────────────────────────────────────────────────────
  const [selectedFile, setSelectedFile]   = useState<File | null>(null)
  const [uploading, setUploading]         = useState(false)
  const [uploadError, setUploadError]     = useState<string | null>(null)
  const [rollbackErrors, setRollbackErrors] = useState<string[]>([])
  const [uploadResult, setUploadResult]   = useState<ImportResultDto | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  // ── Tab 2: Import history ─────────────────────────────────────────────────
  const [importLogs, setImportLogs]           = useState<ImportLogItem[]>([])
  const [importPage, setImportPage]           = useState(1)
  const [importTotalPages, setImportTotalPages] = useState(1)
  const [importCount, setImportCount]         = useState(0)
  const [loadingLogs, setLoadingLogs]         = useState(false)
  const [logsError, setLogsError]             = useState<string | null>(null)
  const [logsVersion, setLogsVersion]         = useState(0)

  // ── Load leave types (SFR-015) ─────────────────────────────────────────────
  useEffect(() => {
    if (activeTab !== 'leave-types') return
    let active = true
    setLoadingTypes(true)
    setTypesError(null)
    fetch('/api/v1/leave-types')
      .then(r => r.json() as Promise<ApiResponse<LeaveTypeItem[]>>)
      .then(json => {
        if (!active) return
        if (json.success && json.data) setLeaveTypes(json.data)
        else setTypesError(json.message ?? 'ไม่สามารถโหลดประเภทการลาได้')
      })
      .catch(() => { if (active) setTypesError('เกิดข้อผิดพลาดในการเชื่อมต่อ') })
      .finally(() => { if (active) setLoadingTypes(false) })
    return () => { active = false }
  }, [activeTab])

  // ── Load import history (SFR-012) ─────────────────────────────────────────
  const loadImportLogs = useCallback(async () => {
    setLoadingLogs(true)
    setLogsError(null)
    try {
      const qs  = new URLSearchParams({ page: String(importPage), pageSize: String(PAGE_SIZE) })
      const res  = await fetch(`/api/v1/hr/outsource-imports?${qs}`)
      const json = await res.json() as ApiResponse<PagedResult<ImportLogItem>>
      if (json.success && json.data) {
        setImportLogs(json.data.items)
        setImportCount(json.data.totalCount)
        setImportTotalPages(json.data.totalPages)
      } else {
        setLogsError(json.message ?? 'ไม่สามารถโหลดประวัติ import ได้')
      }
    } catch {
      setLogsError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setLoadingLogs(false)
    }
  }, [importPage, logsVersion])

  useEffect(() => {
    if (activeTab === 'import') loadImportLogs()
  }, [activeTab, loadImportLogs])

  // ── Upload handler (IF-003, POST /api/v1/hr/outsource-imports) ────────────
  const handleUpload = async () => {
    if (!selectedFile || uploading) return
    setUploading(true)
    setUploadError(null)
    setRollbackErrors([])
    setUploadResult(null)

    try {
      const form = new FormData()
      form.append('file', selectedFile)

      // apiUploadFile: ส่ง X-Employee-Id header + ไม่ set Content-Type (browser ใส่ boundary เอง)
      const res = await apiUploadFile('/api/v1/hr/outsource-imports', form)

      if (res.ok) {
        // 200 OK — partial or full success
        const json = await res.json() as ApiResponse<ImportResultDto>
        if (json.success && json.data) {
          setUploadResult(json.data)
        } else {
          setUploadError(json.message ?? 'นำเข้าข้อมูลไม่สำเร็จ')
        }
      } else if (res.status === 422) {
        // Rollback: >50% rows failed — ApiResponse.Fail with string error array
        const json = await res.json() as ApiResponse<never>
        setUploadError(json.message ?? 'ยกเลิกการนำเข้าทั้งหมด (rows ล้มเหลว > 50%)')
        setRollbackErrors(json.errors ?? [])
      } else {
        const json = await res.json().catch(() => ({})) as ApiResponse<never>
        setUploadError(json.message ?? `เกิดข้อผิดพลาด (HTTP ${res.status})`)
      }

      // Refresh history on any server response (ImportLog always written)
      setImportPage(1)
      setLogsVersion(v => v + 1)
    } catch {
      setUploadError('เกิดข้อผิดพลาดในการเชื่อมต่อ กรุณาลองใหม่')
    } finally {
      setUploading(false)
      setSelectedFile(null)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null
    setSelectedFile(file)
    setUploadError(null)
    setRollbackErrors([])
    setUploadResult(null)
  }

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div>
      {/* Breadcrumb */}
      <nav style={s.breadcrumb} aria-label="breadcrumb">
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span style={s.crumbCurrent}>ตั้งค่าระบบ</span>
      </nav>

      {/* Role guard note */}
      <div style={s.roleNote} role="note">
        <span aria-hidden="true">🔒</span>
        <span>
          <strong>หน้านี้สำหรับ HR เท่านั้น</strong> —
          ระบบ Authentication จะจำกัดสิทธิ์เมื่อ MSAL พร้อมใช้งาน (NFR-005)
        </span>
      </div>

      {/* ── Tab bar ─────────────────────────────────────────────────────────── */}
      <div style={s.tabBar} role="tablist" aria-label="settings tabs">
        {([
          { id: 'leave-types', label: '📋 ประเภทการลา' },
          { id: 'import',      label: '📥 นำเข้าข้อมูล (Import)' },
        ] as const).map(tab => (
          <button
            key={tab.id}
            role="tab"
            aria-selected={activeTab === tab.id}
            style={activeTab === tab.id ? { ...s.tab, ...s.tabActive } : s.tab}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ── Tab 1: Leave Types ─────────────────────────────────────────────── */}
      {activeTab === 'leave-types' && (
        <section style={s.card} role="tabpanel" aria-label="ประเภทการลา">
          <div style={s.cardHeader}>
            <h2 style={s.cardTitle}>ประเภทการลา</h2>
            <span style={s.readOnlyBadge}>อ่านอย่างเดียว — CRUD จะเปิดใน Phase 2</span>
          </div>

          {loadingTypes ? (
            <div style={s.centerBox}>⏳ กำลังโหลด...</div>
          ) : typesError ? (
            <div style={s.errorBox} role="alert">{typesError}</div>
          ) : leaveTypes.length === 0 ? (
            <div style={s.centerBox}>ไม่มีข้อมูลประเภทการลา</div>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table style={s.table} role="table">
                <thead>
                  <tr>
                    <th style={{ ...s.th, width: 44, textAlign: 'center' }}>#</th>
                    <th style={s.th}>ชื่อประเภทการลา</th>
                    <th style={{ ...s.th, textAlign: 'center' }}>สิทธิ์สูงสุด / ปี</th>
                    <th style={{ ...s.th, textAlign: 'center' }}>พนักงาน Outsource</th>
                    <th style={{ ...s.th, textAlign: 'center' }}>สถานะ</th>
                  </tr>
                </thead>
                <tbody>
                  {leaveTypes.map((lt, idx) => (
                    <tr key={lt.leaveTypeId} style={s.tr}>
                      <td style={{ ...s.td, textAlign: 'center', color: 'var(--color-text-muted)' }}>
                        {idx + 1}
                      </td>
                      <td style={s.td}>
                        <span style={{ fontWeight: 500 }}>{lt.leaveTypeName}</span>
                      </td>
                      <td style={{ ...s.td, textAlign: 'center' }}>
                        {lt.maxDaysPerYear != null ? `${lt.maxDaysPerYear} วัน` : '—'}
                      </td>
                      <td style={{ ...s.td, textAlign: 'center' }}>
                        {lt.isAvailableForOutsource === true
                          ? <span style={s.pillGreen}>✓ ได้</span>
                          : <span style={s.pillGray}>— ไม่ได้</span>
                        }
                      </td>
                      <td style={{ ...s.td, textAlign: 'center' }}>
                        {lt.isActive !== false
                          ? <span style={{ ...s.badge, backgroundColor: '#e8f5e9', color: 'var(--color-success)' }}>Active</span>
                          : <span style={{ ...s.badge, backgroundColor: '#f0f0f0', color: 'var(--color-text-muted)' }}>Inactive</span>
                        }
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}

      {/* ── Tab 2: Import ──────────────────────────────────────────────────── */}
      {activeTab === 'import' && (
        <div role="tabpanel" aria-label="นำเข้าข้อมูล">

          {/* Upload section */}
          <section style={s.card}>
            <h2 style={s.cardTitle}>นำเข้าข้อมูลพนักงาน Outsource</h2>
            <p style={s.hint}>
              รองรับเฉพาะไฟล์ <strong>.xlsx</strong> (Excel) ขนาดไม่เกิน <strong>10 MB</strong> —
              ดาวน์โหลด
              {' '}<a href="#" style={{ color: 'var(--color-primary)' }}>Template Excel</a>
            </p>

            {/* File picker row */}
            <div style={s.uploadRow}>
              <input
                ref={fileInputRef}
                type="file"
                accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                style={{ display: 'none' }}
                onChange={handleFileChange}
                aria-label="เลือกไฟล์ Excel สำหรับ import"
              />
              <button
                style={s.btnChooseFile}
                onClick={() => fileInputRef.current?.click()}
                disabled={uploading}
              >
                📁 เลือกไฟล์
              </button>
              <span style={selectedFile ? s.fileNameSelected : s.fileNameEmpty}>
                {selectedFile ? selectedFile.name : 'ยังไม่ได้เลือกไฟล์'}
              </span>
              <button
                style={{ ...s.btnImport, opacity: !selectedFile || uploading ? 0.5 : 1 }}
                disabled={!selectedFile || uploading}
                onClick={handleUpload}
              >
                {uploading ? '⏳ กำลังนำเข้า...' : '▶ เริ่มนำเข้า'}
              </button>
            </div>

            {/* Upload error (400/422/network) */}
            {uploadError && (
              <div style={s.errorBox} role="alert">
                <strong>{uploadError}</strong>
                {rollbackErrors.length > 0 && (
                  <ul style={{ margin: '8px 0 0 16px', lineHeight: 1.8 }}>
                    {rollbackErrors.slice(0, 10).map((e, i) => <li key={i}>{e}</li>)}
                    {rollbackErrors.length > 10 && (
                      <li style={{ color: 'var(--color-text-muted)' }}>...และอีก {rollbackErrors.length - 10} รายการ</li>
                    )}
                  </ul>
                )}
              </div>
            )}

            {/* Upload result (200 OK — full or partial success) */}
            {uploadResult && !uploadError && (
              <div style={s.resultCard}>
                {/* Result header */}
                <div style={s.resultTitle}>
                  {uploadResult.failedRecords === 0
                    ? <span style={{ color: 'var(--color-success)' }}>✅ นำเข้าสำเร็จทั้งหมด</span>
                    : <span style={{ color: 'var(--color-warning)' }}>⚠ นำเข้าเสร็จสิ้น (มีข้อผิดพลาดบางส่วน)</span>
                  }
                </div>

                {/* Stat row */}
                <div style={s.statRow}>
                  <StatChip label="ทั้งหมด"  value={uploadResult.totalRecords}   />
                  <StatChip label="สำเร็จ"    value={uploadResult.successRecords} color="var(--color-success)" />
                  <StatChip label="ล้มเหลว"   value={uploadResult.failedRecords}  color={uploadResult.failedRecords > 0 ? 'var(--color-error)' : undefined} />
                </div>

                {/* Error detail table */}
                {uploadResult.errors.length > 0 && (
                  <div style={{ marginTop: 14 }}>
                    <p style={s.errTableTitle}>
                      รายละเอียดข้อผิดพลาด ({uploadResult.errors.length} รายการ)
                    </p>
                    <div style={{ overflowX: 'auto', maxHeight: 220, overflowY: 'auto', border: '1px solid var(--color-border)', borderRadius: 4 }}>
                      <table style={{ ...s.table, fontSize: 12 }}>
                        <thead>
                          <tr>
                            <th style={{ ...s.th, width: 64 }}>Row</th>
                            <th style={s.th}>Field</th>
                            <th style={s.th}>ข้อผิดพลาด</th>
                          </tr>
                        </thead>
                        <tbody>
                          {uploadResult.errors.map((err, i) => (
                            <tr key={i} style={s.tr}>
                              <td style={{ ...s.td, textAlign: 'center', fontSize: 12 }}>{err.rowNumber}</td>
                              <td style={{ ...s.td, fontFamily: 'monospace', fontSize: 12, color: 'var(--color-primary)' }}>{err.field}</td>
                              <td style={{ ...s.td, fontSize: 12, color: 'var(--color-error)' }}>{err.message}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}
              </div>
            )}
          </section>

          {/* Import history */}
          <section style={{ ...s.card, marginTop: 16 }}>
            <div style={s.cardHeader}>
              <h2 style={s.cardTitle}>ประวัติการ Import</h2>
              {!loadingLogs && (
                <span style={s.countBadge}>{importCount} ครั้ง</span>
              )}
            </div>

            {loadingLogs ? (
              <div style={s.centerBox}>⏳ กำลังโหลด...</div>
            ) : logsError ? (
              <div style={s.errorBox} role="alert">
                {logsError}{' '}
                <button style={s.btnLink} onClick={loadImportLogs}>ลองใหม่</button>
              </div>
            ) : importLogs.length === 0 ? (
              <div style={s.centerBox}>
                <span style={{ fontSize: 28 }}>📂</span>
                <p style={{ color: 'var(--color-text-muted)', marginTop: 8 }}>ยังไม่มีประวัติการ import</p>
              </div>
            ) : (
              <div style={{ overflowX: 'auto' }}>
                <table style={s.table} role="table">
                  <thead>
                    <tr>
                      <th style={s.th}>วันที่ / เวลา</th>
                      <th style={s.th}>ชื่อไฟล์</th>
                      <th style={s.th}>ผู้ import</th>
                      <th style={{ ...s.th, textAlign: 'center' }}>ทั้งหมด</th>
                      <th style={{ ...s.th, textAlign: 'center', color: 'var(--color-success)' }}>สำเร็จ</th>
                      <th style={{ ...s.th, textAlign: 'center', color: 'var(--color-error)' }}>ล้มเหลว</th>
                      <th style={s.th}>สถานะ</th>
                    </tr>
                  </thead>
                  <tbody>
                    {importLogs.map(log => {
                      const bd = importBadge(log)
                      return (
                        <tr key={log.importLogId} style={s.tr}>
                          <td style={{ ...s.td, whiteSpace: 'nowrap', color: 'var(--color-text-muted)', fontSize: 12 }}>
                            {formatDateTime(log.createdAt)}
                          </td>
                          <td style={s.td}>
                            <span title={log.fileName} style={s.ellipsis}>{log.fileName}</span>
                          </td>
                          <td style={{ ...s.td, color: 'var(--color-text-muted)' }}>{log.importedBy}</td>
                          <td style={{ ...s.td, textAlign: 'center' }}>{log.totalRecords}</td>
                          <td style={{ ...s.td, textAlign: 'center', color: 'var(--color-success)', fontWeight: 600 }}>
                            {log.successRecords}
                          </td>
                          <td style={{ ...s.td, textAlign: 'center', color: log.failedRecords > 0 ? 'var(--color-error)' : 'inherit', fontWeight: log.failedRecords > 0 ? 600 : 400 }}>
                            {log.failedRecords}
                          </td>
                          <td style={s.td}>
                            <span style={{ ...s.badge, backgroundColor: bd.bg, color: bd.color }}>
                              {bd.label}
                            </span>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}

            {/* Pagination */}
            {!loadingLogs && !logsError && importTotalPages > 1 && (
              <div style={s.pagination} role="navigation" aria-label="pagination">
                <button style={s.pageBtn} disabled={importPage === 1} onClick={() => setImportPage(p => p - 1)}>
                  ‹ ก่อนหน้า
                </button>
                <span style={s.pageInfo}>หน้า <strong>{importPage}</strong> / {importTotalPages}</span>
                <button style={s.pageBtn} disabled={importPage === importTotalPages} onClick={() => setImportPage(p => p + 1)}>
                  ถัดไป ›
                </button>
              </div>
            )}
          </section>
        </div>
      )}
    </div>
  )
}

// ── Sub-component: stat chip ──────────────────────────────────────────────────

function StatChip({ label, value, color }: { label: string; value: number; color?: string }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', minWidth: 64 }}>
      <span style={{ fontSize: 24, fontWeight: 700, lineHeight: 1, color: color ?? 'var(--color-text)' }}>
        {value}
      </span>
      <span style={{ fontSize: 11, color: 'var(--color-text-muted)', marginTop: 4 }}>{label}</span>
    </div>
  )
}

// ── Styles ────────────────────────────────────────────────────────────────────

const s: Record<string, React.CSSProperties> = {
  breadcrumb:   { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 16, fontSize: 13 },
  crumbLink:    { color: 'var(--color-primary)' },
  crumbSep:     { color: 'var(--color-text-muted)', margin: '0 4px' },
  crumbCurrent: { color: 'var(--color-text)', fontWeight: 500 },

  roleNote: {
    display: 'flex',
    alignItems: 'center',
    gap: 8,
    backgroundColor: '#fffbe6',
    border: '1px solid #ffe58f',
    borderRadius: 6,
    padding: '10px 14px',
    fontSize: 13,
    color: '#7d5900',
    marginBottom: 16,
  },

  // ── Tabs ────────────────────────────────────────────────────────
  tabBar: {
    display: 'flex',
    gap: 0,
    borderBottom: '2px solid var(--color-border)',
    marginBottom: 16,
  },
  tab: {
    padding: '10px 20px',
    border: 'none',
    borderBottom: '3px solid transparent',
    marginBottom: -2,
    backgroundColor: 'transparent',
    color: 'var(--color-text-muted)',
    fontSize: 13,
    fontWeight: 500,
    cursor: 'pointer',
    whiteSpace: 'nowrap',
  },
  tabActive: {
    color: 'var(--color-primary)',
    borderBottomColor: 'var(--color-primary)',
  },

  // ── Card ────────────────────────────────────────────────────────
  card: {
    backgroundColor: 'var(--color-surface)',
    border: '1px solid var(--color-border)',
    borderRadius: 8,
    padding: '18px 22px',
  },
  cardHeader: { display: 'flex', alignItems: 'center', gap: 10, marginBottom: 16 },
  cardTitle: { fontSize: 15, fontWeight: 600, color: 'var(--color-text)' },

  readOnlyBadge: {
    fontSize: 11,
    backgroundColor: '#f0f0f0',
    color: 'var(--color-text-muted)',
    borderRadius: 4,
    padding: '2px 8px',
  },
  countBadge: {
    fontSize: 12,
    backgroundColor: 'var(--color-bg)',
    color: 'var(--color-text-muted)',
    border: '1px solid var(--color-border)',
    borderRadius: 10,
    padding: '2px 10px',
  },

  // ── Table ────────────────────────────────────────────────────────
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th: {
    textAlign: 'left',
    padding: '9px 12px',
    borderBottom: '2px solid var(--color-border)',
    fontSize: 12,
    fontWeight: 600,
    color: 'var(--color-text-muted)',
    whiteSpace: 'nowrap',
    backgroundColor: 'var(--color-bg)',
  },
  tr:  { borderBottom: '1px solid var(--color-border)' },
  td:  { padding: '10px 12px', color: 'var(--color-text)', verticalAlign: 'middle' },

  badge: {
    display: 'inline-block',
    padding: '3px 10px',
    borderRadius: 10,
    fontSize: 12,
    fontWeight: 500,
    whiteSpace: 'nowrap',
  },
  pillGreen: { fontSize: 12, color: 'var(--color-success)', fontWeight: 500 },
  pillGray:  { fontSize: 12, color: 'var(--color-text-muted)' },
  ellipsis: {
    display: 'block',
    maxWidth: 220,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },

  // ── Upload area ──────────────────────────────────────────────────
  hint: { fontSize: 12, color: 'var(--color-text-muted)', marginBottom: 16, lineHeight: 1.6 },
  uploadRow: {
    display: 'flex',
    alignItems: 'center',
    gap: 10,
    flexWrap: 'wrap',
  },
  btnChooseFile: {
    padding: '8px 16px',
    border: '1px solid var(--color-primary)',
    borderRadius: 4,
    backgroundColor: 'var(--color-primary-light)',
    color: 'var(--color-primary)',
    fontSize: 13,
    fontWeight: 500,
    whiteSpace: 'nowrap',
  },
  fileNameSelected: {
    fontSize: 13,
    color: 'var(--color-text)',
    flex: 1,
    minWidth: 0,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  fileNameEmpty: {
    fontSize: 13,
    color: 'var(--color-text-muted)',
    flex: 1,
  },
  btnImport: {
    padding: '8px 20px',
    backgroundColor: 'var(--color-primary)',
    color: '#fff',
    border: 'none',
    borderRadius: 4,
    fontSize: 13,
    fontWeight: 500,
    whiteSpace: 'nowrap',
  },

  // ── Result card ───────────────────────────────────────────────────
  resultCard: {
    marginTop: 16,
    border: '1px solid var(--color-border)',
    borderRadius: 6,
    padding: '16px 18px',
    backgroundColor: 'var(--color-bg)',
  },
  resultTitle: { fontSize: 14, fontWeight: 600, marginBottom: 14 },
  statRow:     { display: 'flex', gap: 24, marginBottom: 4, alignItems: 'flex-end' },
  errTableTitle: {
    fontSize: 12,
    fontWeight: 600,
    color: 'var(--color-text-muted)',
    marginBottom: 6,
  },

  // ── Misc ─────────────────────────────────────────────────────────
  centerBox: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '40px 24px',
    color: 'var(--color-text-muted)',
    fontSize: 14,
    gap: 4,
  },
  errorBox: {
    backgroundColor: '#fde8e8',
    border: '1px solid var(--color-error)',
    borderRadius: 4,
    padding: '10px 14px',
    color: 'var(--color-error)',
    fontSize: 13,
    marginTop: 12,
    lineHeight: 1.6,
  },
  btnLink: {
    background: 'none',
    border: 'none',
    color: 'var(--color-primary)',
    fontSize: 13,
    textDecoration: 'underline',
    padding: '0 4px',
    cursor: 'pointer',
  },

  pagination: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 16,
    paddingTop: 16,
    borderTop: '1px solid var(--color-border)',
    marginTop: 8,
  },
  pageBtn: {
    padding: '6px 14px',
    border: '1px solid var(--color-border)',
    borderRadius: 4,
    backgroundColor: 'var(--color-surface)',
    color: 'var(--color-text)',
    fontSize: 13,
  },
  pageInfo: { fontSize: 13, color: 'var(--color-text-muted)' },
}
