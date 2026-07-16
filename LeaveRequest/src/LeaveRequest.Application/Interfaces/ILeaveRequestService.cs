namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface ILeaveRequestService
{
    // SCR-003: ยื่นคำร้อง
    Task<LeaveRequestResult> SubmitLeaveRequestAsync(
        string employeeId,
        CreateLeaveRequestDto request,
        CancellationToken ct = default);

    // SCR-003: รายการคำร้องของฉัน
    Task<PagedResult<LeaveRequestSummaryDto>> GetMyRequestsAsync(
        string employeeId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // SCR-005: รายละเอียดคำร้อง
    Task<LeaveRequestDetailDto> GetDetailAsync(
        Guid leaveRequestId,
        CancellationToken ct = default);

    // SCR-005 (SF-013): audit trail timeline ของคำร้อง
    Task<IReadOnlyList<TimelineEventDto>> GetTimelineAsync(
        Guid leaveRequestId,
        CancellationToken ct = default);

    // SCR-006: ยกเลิกคำร้อง (Pending → Cancelled ทันที; Approved → CancelRequest)
    Task<string> CancelAsync(
        Guid leaveRequestId,
        string employeeId,
        string? reason,
        CancellationToken ct = default);

    // SCR-004: Manager อนุมัติ
    Task ApproveAsync(
        Guid leaveRequestId,
        string managerId,
        string? comment,
        CancellationToken ct = default);

    // SCR-004: Manager ปฏิเสธ
    Task RejectAsync(
        Guid leaveRequestId,
        string managerId,
        string? comment,
        CancellationToken ct = default);

    // SCR-007: Manager อนุมัติ/ปฏิเสธ CancelRequest
    Task ApproveCancelAsync(
        Guid cancelRequestId,
        string managerId,
        string? comment,
        CancellationToken ct = default);

    Task RejectCancelAsync(
        Guid cancelRequestId,
        string managerId,
        string? comment,
        CancellationToken ct = default);

    // SCR-004: pending approvals list สำหรับ manager
    Task<PagedResult<PendingApprovalDto>> GetPendingByManagerAsync(
        string managerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // SCR-004 (SF-004): รายการที่ manager ดำเนินการแล้ว (Approved/Rejected)
    Task<PagedResult<HrLeaveRequestDto>> GetProcessedByManagerAsync(
        string managerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // SCR-007: pending cancel requests สำหรับ manager
    Task<PagedResult<PendingCancelRequestDto>> GetCancelRequestsByManagerAsync(
        string managerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // SCR-008: HR รายการทั้งหมด
    Task<PagedResult<HrLeaveRequestDto>> GetAllForHrAsync(
        string? status,
        string? department,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
