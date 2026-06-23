namespace LeaveRequest.Application.DTOs;

using LeaveRequest.Domain.Enums;

// ─── Input ──────────────────────────────────────────────────────────────────

public record CreateLeaveRequestDto(
    byte LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsHalfDay,
    string? HalfDayPeriod,
    string? Reason,
    List<Guid> AttachmentIds
);

// ─── Output ─────────────────────────────────────────────────────────────────

public record LeaveRequestResult(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    LeaveStatus Status,
    string Message
);

// ─── Leave Request List Item (GET /leave-requests?employeeId=) ────────────────

public record LeaveRequestSummaryDto(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    bool IsHalfDay,
    string? Reason,
    string Status,
    DateTime CreatedAt
);

// ─── Leave Request Detail (GET /leave-requests/{id}) ─────────────────────────

public record LeaveRequestDetailDto(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string EmployeeId,
    string EmployeeFullNameTh,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    bool IsHalfDay,
    string? HalfDayPeriod,
    string? Reason,
    string Status,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? RejectedBy,
    DateTime? RejectedAt,
    string? RejectionReason,
    DateTime CreatedAt
);

// ─── Pending Approval (GET /approvals/pending) ───────────────────────────────

public record PendingApprovalDto(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string EmployeeId,
    string EmployeeFullNameTh,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    string? Reason,
    DateTime CreatedAt
);

// ─── Pending Cancel Request (GET /approvals/cancel-requests) ─────────────────

public record PendingCancelRequestDto(
    Guid CancelRequestId,
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string EmployeeId,
    string EmployeeFullNameTh,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    string? CancelReason,
    DateTime CancelRequestedAt,
    DateTime SlaDeadline
);

// ─── Approve / Reject inputs ──────────────────────────────────────────────────

public record ApproveRejectDto(string? Comment);

// ─── Leave Balance Dashboard (GET /leave-balances/dashboard) ─────────────────

public record LeaveBalanceDashboardDto(
    string EmployeeId,
    int LeaveYear,
    IReadOnlyList<LeaveBalanceItemDto> Balances
);

public record LeaveBalanceItemDto(
    byte LeaveTypeId,
    string TypeCode,
    string TypeNameTh,
    decimal EntitledDays,
    decimal UsedDays,
    decimal PendingDays,
    decimal CarriedForwardDays,
    decimal RemainingDays
);

// ─── HR Leave Request List ────────────────────────────────────────────────────

public record HrLeaveRequestDto(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string EmployeeId,
    string EmployeeFullNameTh,
    string? Department,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    string Status,
    DateTime CreatedAt
);
