namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface ILeaveReportService
{
    Task<PagedResult<LeaveHistoryItemDto>> GetLeaveHistoryAsync(
        LeaveHistoryFilterRequest filter,
        CancellationToken ct = default);
}
