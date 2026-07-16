// ── Enums ──────────────────────────────────────────────────────────────────────
// Match C# LeaveStatus enum string values (Status.ToString())

export type LeaveStatus =
  | 'Pending'         // รอการอนุมัติ
  | 'Approved'        // อนุมัติแล้ว
  | 'Rejected'        // ปฏิเสธ
  | 'Cancelled'       // ยกเลิกแล้ว
  | 'CancelRequested' // ขอยกเลิก (Approved → รอ Manager อนุมัติการยกเลิก)
  | 'Escalated'       // Escalated (SLA)

// ── Domain DTOs ────────────────────────────────────────────────────────────────

export interface LeaveType {
  leaveTypeId: number
  leaveTypeName: string
  typeNameTh?: string
  maxDaysPerYear?: number
  isAvailableForOutsource?: boolean
  requiresMedicalCert?: boolean
}

export interface AttachmentSummary {
  attachmentId: string
  fileName: string
  contentType: string
  fileSize: number
}

export interface LeaveRequestSummary {
  leaveRequestId: string       // Guid
  leaveRequestRef: string
  leaveTypeName: string
  startDate: string            // "YYYY-MM-DD"
  endDate: string              // "YYYY-MM-DD"
  durationDays: number
  isHalfDay: boolean
  reason?: string
  status: LeaveStatus
  createdAt: string            // ISO datetime
}

export interface LeaveRequestDetail extends LeaveRequestSummary {
  employeeId: string
  employeeFullNameTh: string
  halfDayPeriod?: string
  approvedBy?: string
  approvedAt?: string
  rejectedBy?: string
  rejectedAt?: string
  rejectionReason?: string
  attachments?: AttachmentSummary[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface LeaveBalanceItem {
  leaveTypeId: number
  typeCode: string
  typeNameTh: string
  entitledDays: number
  usedDays: number
  pendingDays: number
  carriedForwardDays: number
  remainingDays: number
}

export interface LeaveBalanceDashboard {
  employeeId: string
  leaveYear: number
  isProbation?: boolean
  balances: LeaveBalanceItem[]
}

// ── Request body (POST /api/v1/leave-requests) ─────────────────────────────────

export interface CreateLeaveRequestBody {
  leaveTypeId: number
  startDate: string       // "YYYY-MM-DD"
  endDate: string         // "YYYY-MM-DD"
  isHalfDay: boolean
  halfDayPeriod?: 'AM' | 'PM'
  reason?: string
  attachmentIds: string[]
}

// ── API envelope ────────────────────────────────────────────────────────────────
// Mirrors ApiResponse<T> from LeaveRequest.API/Models

export interface ApiResponse<T> {
  success: boolean
  data?: T
  errorCode?: string
  message?: string
  errors?: string[]
  metadata?: {
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
  }
}

// ── Status config helper ────────────────────────────────────────────────────────

export const STATUS_CONFIG: Record<string, { label: string; bg: string; color: string }> = {
  Pending:         { label: 'รอการอนุมัติ',  bg: '#fff4e5', color: '#d97706' },
  Approved:        { label: 'อนุมัติแล้ว',   bg: '#e8f5e9', color: '#15803d' },
  Rejected:        { label: 'ปฏิเสธ',         bg: '#fde8e8', color: '#dc2626' },
  Cancelled:       { label: 'ยกเลิกแล้ว',    bg: '#f0f0f0', color: '#6b7280' },
  CancelRequested: { label: 'ขอยกเลิก',       bg: '#fef3c7', color: '#b45309' },
  Escalated:       { label: 'Escalated',       bg: '#fde8e8', color: '#9f1239' },
}

export function statusLabel(status: string): string {
  return STATUS_CONFIG[status]?.label ?? status
}

export function formatDate(iso: string | null | undefined): string {
  if (!iso) return '—'
  const d = iso.split('T')[0]
  const [y, m, dd] = d.split('-')
  return `${dd}/${m}/${y}`
}
