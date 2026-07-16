// SCR-003: Submit Leave Request + My Requests List (SFR-003, SFR-006, SFR-007)
import { useState, useEffect, useCallback, useMemo, useRef } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { apiGet, apiPost, apiUploadFile } from '../../api'
import type { ApiResponse, LeaveType, LeaveRequestSummary, CreateLeaveRequestBody, PagedResult } from '../../types/leaveRequest'
import { STATUS_CONFIG, formatDate } from '../../types/leaveRequest'
import CancelLeaveModal, { type CancelTarget } from '../../components/shared/CancelLeaveModal'

const PAGE_SIZE = 10

function calcDays(start: string, end: string): number {
  if (!start || !end) return 0
  const diff = new Date(end).getTime() - new Date(start).getTime()
  return diff < 0 ? 0 : Math.floor(diff / 86_400_000) + 1
}

export default function LeaveRequestListPage() {
  const { user } = useAuth()
  const employeeId = user?.employeeId ?? ''

  // Leave types
  const [leaveTypes, setLeaveTypes]   = useState<LeaveType[]>([])
  const [loadingTypes, setLoadingTypes] = useState(true)

  // My requests list
  const [requests, setRequests]       = useState<LeaveRequestSummary[]>([])
  const [totalCount, setTotalCount]   = useState(0)
  const [totalPages, setTotalPages]   = useState(1)
  const [page, setPage]               = useState(1)
  const [loadingList, setLoadingList] = useState(true)
  const [listError, setListError]     = useState<string | null>(null)
  const [refreshVer, setRefreshVer]   = useState(0)

  // Submit form
  const [leaveTypeId, setLeaveTypeId] = useState('')
  const [startDate, setStartDate]     = useState('')
  const [endDate, setEndDate]         = useState('')
  const [reason, setReason]           = useState('')
  const [isHalfDay, setIsHalfDay]     = useState(false)
  const [halfDayPeriod, setHalfDayPeriod] = useState<'AM' | 'PM'>('AM')
  const [certFile, setCertFile]       = useState<File | null>(null)
  const certInputRef = useRef<HTMLInputElement>(null)
  const [submitting, setSubmitting]   = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitErrors, setSubmitErrors] = useState<string[]>([])
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null)

  // Cancel (SF-007 / SF-008 — via modal)
  const [cancelTarget, setCancelTarget] = useState<CancelTarget | null>(null)
  const [cancelSuccess, setCancelSuccess] = useState<string | null>(null)

  const durationDays = useMemo(() => calcDays(startDate, endDate), [startDate, endDate])
  const selectedLeaveType = useMemo(
    () => leaveTypes.find(lt => lt.leaveTypeId === Number(leaveTypeId)),
    [leaveTypes, leaveTypeId])
  // VR-007 / BR-006: ลาที่ต้องใบรับรองแพทย์ ตั้งแต่ 3 วันขึ้นไป ต้องแนบไฟล์
  const certRequired = !!selectedLeaveType?.requiresMedicalCert && durationDays >= 3

  // Load leave types
  useEffect(() => {
    let active = true
    apiGet<ApiResponse<LeaveType[]>>('/api/v1/leave-types')
      .then(json => { if (active && json.success) setLeaveTypes(json.data ?? []) })
      .catch(() => {})
      .finally(() => { if (active) setLoadingTypes(false) })
    return () => { active = false }
  }, [])

  // Load my requests
  const loadRequests = useCallback(async () => {
    if (!employeeId) return
    setLoadingList(true)
    setListError(null)
    try {
      const qs = new URLSearchParams({ employeeId, page: String(page), pageSize: String(PAGE_SIZE) })
      const json = await apiGet<ApiResponse<PagedResult<LeaveRequestSummary>>>(`/api/v1/leave-requests?${qs}`)
      if (json.success && json.data) {
        setRequests(json.data.items)
        setTotalCount(json.data.totalCount)
        setTotalPages(json.data.totalPages)
      } else {
        setListError(json.message ?? 'ไม่สามารถโหลดรายการได้')
      }
    } catch {
      setListError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setLoadingList(false)
    }
  }, [employeeId, page, refreshVer])

  useEffect(() => { loadRequests() }, [loadRequests])

  // Submit
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitError(null); setSubmitErrors([]); setSubmitSuccess(null)
    const errs: string[] = []
    if (!leaveTypeId)          errs.push('กรุณาเลือกประเภทการลา')
    if (!startDate)            errs.push('กรุณาระบุวันเริ่มลา')
    if (!endDate)              errs.push('กรุณาระบุวันสิ้นสุด')
    if (!reason.trim())        errs.push('กรุณาระบุเหตุผลการลา')
    if (startDate && endDate && endDate < startDate) errs.push('วันสิ้นสุดต้องไม่ก่อนวันเริ่มลา')
    if (certRequired && !certFile) errs.push('การลานี้ตั้งแต่ 3 วันขึ้นไป ต้องแนบใบรับรองแพทย์')
    if (errs.length) { setSubmitErrors(errs); return }

    setSubmitting(true)
    try {
      // IF-004: upload ใบรับรองแพทย์ก่อน แล้วแนบ id เข้าคำขอลา
      let attachmentIds: string[] = []
      if (certFile) {
        const form = new FormData()
        form.append('file', certFile)
        const uploadRes = await apiUploadFile('/api/v1/attachments', form)
        const uploadJson = await uploadRes.json() as ApiResponse<{ attachmentId: string }>
        if (!uploadRes.ok || !uploadJson.success || !uploadJson.data) {
          setSubmitError(uploadJson.message ?? 'อัปโหลดใบรับรองแพทย์ไม่สำเร็จ')
          setSubmitting(false)
          return
        }
        attachmentIds = [uploadJson.data.attachmentId]
      }

      const body: CreateLeaveRequestBody = {
        leaveTypeId: Number(leaveTypeId), startDate, endDate,
        isHalfDay, halfDayPeriod: isHalfDay ? halfDayPeriod : undefined,
        reason: reason.trim(), attachmentIds,
      }
      const json = await apiPost<ApiResponse<{ leaveRequestRef?: string }>>('/api/v1/leave-requests', body)
      if (json.success) {
        setSubmitSuccess(`ยื่นคำร้องสำเร็จ${json.data?.leaveRequestRef ? ` (${json.data.leaveRequestRef})` : ''}`)
        setLeaveTypeId(''); setStartDate(''); setEndDate(''); setReason(''); setIsHalfDay(false); setHalfDayPeriod('AM')
        setCertFile(null); if (certInputRef.current) certInputRef.current.value = ''
        setPage(1); setRefreshVer(v => v + 1)
      } else {
        setSubmitError(json.message ?? 'ยื่นคำร้องไม่สำเร็จ')
        setSubmitErrors(json.errors ?? [])
      }
    } catch {
      setSubmitError('เกิดข้อผิดพลาดในการเชื่อมต่อ')
    } finally {
      setSubmitting(false)
    }
  }

  // Cancel (SCR-006) — เปิด modal เพื่อกรอกเหตุผล (SF-007/SF-008)
  const handleCancelSuccess = (message: string) => {
    setCancelTarget(null)
    setCancelSuccess(message)
    setPage(1); setRefreshVer(v => v + 1)
    setTimeout(() => setCancelSuccess(null), 4000)
  }

  const canCancel = (status: string) => status === 'Pending' || status === 'Approved'

  return (
    <div>
      <nav style={s.breadcrumb}>
        <Link to="/" style={s.crumbLink}>หน้าหลัก</Link>
        <span style={s.crumbSep}>›</span>
        <span>ยื่นคำร้องขอลา</span>
      </nav>

      {cancelSuccess && <div style={s.successBanner}>{cancelSuccess}</div>}

      {/* Submit form */}
      <section style={s.card}>
        <h2 style={s.cardTitle}>ยื่นคำร้องขอลาใหม่</h2>
        <form onSubmit={handleSubmit} noValidate>
          <div style={s.formGrid}>
            <div style={s.field}>
              <label style={s.label}>ประเภทการลา <span style={{ color: '#dc2626' }}>*</span></label>
              <select style={s.select} value={leaveTypeId} onChange={e => setLeaveTypeId(e.target.value)} disabled={loadingTypes || submitting}>
                <option value="">{loadingTypes ? 'กำลังโหลด...' : '— เลือกประเภทการลา —'}</option>
                {leaveTypes.map(lt => (
                  <option key={lt.leaveTypeId} value={lt.leaveTypeId}>
                    {lt.typeNameTh ?? lt.leaveTypeName}
                  </option>
                ))}
              </select>
            </div>
            <div style={s.field}>
              <label style={s.label}>วันเริ่มลา <span style={{ color: '#dc2626' }}>*</span></label>
              <input type="date" style={s.input} value={startDate}
                onChange={e => { setStartDate(e.target.value); if (endDate && e.target.value > endDate) setEndDate(e.target.value) }}
                disabled={submitting} />
            </div>
            <div style={s.field}>
              <label style={s.label}>วันสิ้นสุด <span style={{ color: '#dc2626' }}>*</span></label>
              <input type="date" style={s.input} value={endDate} min={startDate || undefined}
                onChange={e => setEndDate(e.target.value)} disabled={submitting} />
            </div>
            <div style={s.field}>
              <label style={s.label}>จำนวนวัน</label>
              <input type="text" style={{ ...s.input, backgroundColor: '#f9fafb' }}
                value={durationDays > 0 ? `${durationDays} วัน` : '—'} readOnly />
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginTop: 12 }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer' }}>
              <input type="checkbox" checked={isHalfDay} onChange={e => setIsHalfDay(e.target.checked)} disabled={submitting} />
              ลาครึ่งวัน
            </label>
            {isHalfDay && (
              <select style={{ ...s.select, width: 'auto' }} value={halfDayPeriod}
                onChange={e => setHalfDayPeriod(e.target.value as 'AM' | 'PM')} disabled={submitting}>
                <option value="AM">ช่วงเช้า (AM)</option>
                <option value="PM">ช่วงบ่าย (PM)</option>
              </select>
            )}
          </div>
          <div style={{ ...s.field, marginTop: 12 }}>
            <label style={s.label}>เหตุผลการลา <span style={{ color: 'var(--color-danger)' }}>*</span></label>
            <textarea style={s.textarea} rows={3} maxLength={500} value={reason} required
              onChange={e => setReason(e.target.value)} placeholder="ระบุเหตุผลการลา" disabled={submitting} />
            <small style={{ color: 'var(--color-text-muted)', fontSize: 11 }}>{reason.length}/500</small>
          </div>

          {/* SF-003 / IF-004: แนบใบรับรองแพทย์ (แสดงเมื่อประเภทลาต้องใช้) */}
          {selectedLeaveType?.requiresMedicalCert && (
            <div style={{ ...s.field, marginTop: 12 }}>
              <label style={s.label}>
                ใบรับรองแพทย์ {certRequired && <span style={{ color: 'var(--color-danger)' }}>*</span>}
                <span style={{ fontWeight: 400, color: 'var(--color-text-muted)' }}> (PDF, JPG, PNG — ไม่เกิน 5 MB)</span>
              </label>
              <input
                ref={certInputRef}
                type="file"
                accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
                onChange={e => setCertFile(e.target.files?.[0] ?? null)}
                disabled={submitting}
                style={{ fontSize: 13 }}
              />
              {certRequired && !certFile && (
                <small style={{ color: 'var(--color-danger)', fontSize: 11 }}>
                  การลานี้ตั้งแต่ 3 วันขึ้นไป ต้องแนบใบรับรองแพทย์ก่อนยื่น
                </small>
              )}
            </div>
          )}

          {(submitError || submitErrors.length > 0) && (
            <div style={s.errorBox}>
              {submitError && <p style={{ fontWeight: 600, margin: 0 }}>{submitError}</p>}
              {submitErrors.map((e, i) => <p key={i} style={{ margin: '4px 0 0' }}>• {e}</p>)}
            </div>
          )}
          {submitSuccess && <div style={s.successBox}>{submitSuccess}</div>}
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
            <button style={s.btnPrimary} type="submit" disabled={submitting}>
              {submitting ? 'กำลังยื่น...' : 'ยื่นคำร้อง'}
            </button>
          </div>
        </form>
      </section>

      {/* My requests list */}
      <section style={{ ...s.card, marginTop: 20 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 14 }}>
          <h2 style={{ ...s.cardTitle, margin: 0 }}>รายการคำร้องของฉัน</h2>
          {!loadingList && <span style={s.countBadge}>{totalCount} รายการ</span>}
        </div>
        {loadingList ? (
          <div style={s.center}>⏳ กำลังโหลด...</div>
        ) : listError ? (
          <div style={s.errorBox}>{listError} <button style={s.btnLink} onClick={loadRequests}>ลองใหม่</button></div>
        ) : requests.length === 0 ? (
          <div style={s.center}>📋 ยังไม่มีคำร้องขอลา</div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={s.table}>
              <thead>
                <tr>
                  {['ประเภทการลา','วันที่','วัน','เหตุผล','สถานะ','วันที่ยื่น',''].map(h => (
                    <th key={h} style={s.th}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {requests.map(req => {
                  const cfg = STATUS_CONFIG[req.status] ?? STATUS_CONFIG.Cancelled
                  return (
                    <tr key={req.leaveRequestId} style={s.tr}>
                      <td style={s.td}>{req.leaveTypeName}</td>
                      <td style={{ ...s.td, whiteSpace: 'nowrap' }}>
                        {formatDate(req.startDate)}{req.startDate !== req.endDate ? ` – ${formatDate(req.endDate)}` : ''}
                      </td>
                      <td style={{ ...s.td, textAlign: 'center' }}>{req.durationDays}</td>
                      <td style={{ ...s.td, maxWidth: 180 }}>
                        <span style={s.ellipsis} title={req.reason ?? ''}>
                          {req.reason || <span style={{ color: 'var(--color-text-muted)' }}>—</span>}
                        </span>
                      </td>
                      <td style={s.td}>
                        <span style={{ ...s.badge, backgroundColor: cfg.bg, color: cfg.color }}>
                          {cfg.label}
                        </span>
                      </td>
                      <td style={{ ...s.td, whiteSpace: 'nowrap', color: 'var(--color-text-muted)' }}>
                        {formatDate(req.createdAt)}
                      </td>
                      <td style={s.td}>
                        <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
                          <Link to={`/leave-requests/${req.leaveRequestId}`} style={s.btnDetail}>รายละเอียด</Link>
                          {canCancel(req.status) && (
                            <button style={s.btnDanger} onClick={() => setCancelTarget({
                              leaveRequestId: req.leaveRequestId,
                              leaveRequestRef: req.leaveRequestRef,
                              status: req.status,
                            })}>
                              ยกเลิก
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
        {!loadingList && totalPages > 1 && (
          <div style={s.pagination}>
            <button style={s.pageBtn} disabled={page === 1} onClick={() => setPage(p => p - 1)}>‹</button>
            <span style={{ fontSize: 13 }}>หน้า {page} / {totalPages}</span>
            <button style={s.pageBtn} disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>›</button>
          </div>
        )}
      </section>

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

const s: Record<string, React.CSSProperties> = {
  breadcrumb: { display: 'flex', alignItems: 'center', gap: 4, marginBottom: 20, fontSize: 13 },
  crumbLink: { color: 'var(--color-primary)' },
  crumbSep:  { color: 'var(--color-text-muted)', margin: '0 4px' },
  card: { backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8, padding: '20px 24px' },
  cardTitle: { fontSize: 15, fontWeight: 600, marginBottom: 16 },
  formGrid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '12px 16px' },
  field:    { display: 'flex', flexDirection: 'column', gap: 4 },
  label:    { fontSize: 12, fontWeight: 500, color: 'var(--color-text-muted)' },
  select:   { padding: '8px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, backgroundColor: 'var(--color-surface)' },
  input:    { padding: '8px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13 },
  textarea: { padding: '8px 10px', border: '1px solid var(--color-border)', borderRadius: 4, fontSize: 13, fontFamily: 'inherit', resize: 'vertical' },
  errorBox: { backgroundColor: '#fde8e8', border: '1px solid #fca5a5', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#dc2626', marginTop: 12 },
  successBox: { backgroundColor: '#e8f5e9', border: '1px solid #86efac', borderRadius: 4, padding: '10px 14px', fontSize: 13, color: '#15803d', marginTop: 12 },
  btnPrimary: { padding: '9px 22px', backgroundColor: 'var(--color-primary)', color: '#fff', border: 'none', borderRadius: 4, fontSize: 13, fontWeight: 500, minWidth: 100, cursor: 'pointer' },
  btnDanger:  { padding: '4px 10px', backgroundColor: 'transparent', color: '#dc2626', border: '1px solid #dc2626', borderRadius: 4, fontSize: 12, cursor: 'pointer', whiteSpace: 'nowrap' },
  btnDetail:  { padding: '4px 10px', backgroundColor: 'transparent', color: 'var(--color-primary)', border: '1px solid var(--color-primary)', borderRadius: 4, fontSize: 12, textDecoration: 'none', whiteSpace: 'nowrap' },
  successBanner: { backgroundColor: '#e8f5e9', border: '1px solid #86efac', borderRadius: 6, padding: '10px 16px', fontSize: 13, color: '#15803d', marginBottom: 12 },
  btnLink:    { background: 'none', border: 'none', color: 'var(--color-primary)', textDecoration: 'underline', fontSize: 13, cursor: 'pointer', padding: '0 4px' },
  countBadge: { fontSize: 12, backgroundColor: 'var(--color-primary-light)', color: 'var(--color-primary)', borderRadius: 10, padding: '2px 10px' },
  center: { display: 'flex', justifyContent: 'center', padding: '32px 0', color: 'var(--color-text-muted)', fontSize: 14 },
  table: { width: '100%', borderCollapse: 'collapse', fontSize: 13 },
  th:    { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid var(--color-border)', fontSize: 12, fontWeight: 600, color: 'var(--color-text-muted)', whiteSpace: 'nowrap', backgroundColor: 'var(--color-bg)' },
  tr:    { borderBottom: '1px solid var(--color-border)' },
  td:    { padding: '10px 12px', verticalAlign: 'middle' },
  badge: { display: 'inline-block', padding: '3px 10px', borderRadius: 10, fontSize: 12, fontWeight: 500, whiteSpace: 'nowrap' },
  ellipsis: { display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 180 },
  pagination: { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14, paddingTop: 16, borderTop: '1px solid var(--color-border)', marginTop: 8 },
  pageBtn: { padding: '5px 12px', border: '1px solid var(--color-border)', borderRadius: 4, backgroundColor: 'var(--color-surface)', cursor: 'pointer' },
}
