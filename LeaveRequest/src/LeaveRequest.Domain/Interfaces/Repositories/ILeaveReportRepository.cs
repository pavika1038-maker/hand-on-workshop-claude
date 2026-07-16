namespace LeaveRequest.Domain.Interfaces.Repositories;

using global::LeaveRequest.Domain.Enums;
using LeaveRequestEntity = global::LeaveRequest.Domain.Entities.LeaveRequest;

public record LeaveHistoryQuery(
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? EmployeeId = null,
    byte? LeaveTypeId = null,
    LeaveStatus? Status = null,
    string? Department = null,
    EmployeeType? EmployeeType = null
);

public interface ILeaveReportRepository
{
    Task<(IReadOnlyList<LeaveRequestEntity> Items, int TotalCount)> GetLeaveHistoryAsync(
        LeaveHistoryQuery query,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // RP-001 (SF-014): ดึงทุก record ที่ match filter (ไม่ paginate) เพื่อ aggregate สรุป
    Task<IReadOnlyList<LeaveRequestEntity>> GetForSummaryAsync(
        LeaveHistoryQuery query,
        CancellationToken ct = default);
}
