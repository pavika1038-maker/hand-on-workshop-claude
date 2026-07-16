namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface ILeaveReportService
{
    Task<PagedResult<LeaveHistoryItemDto>> GetLeaveHistoryAsync(
        LeaveHistoryFilterRequest filter,
        CancellationToken ct = default);

    // SF-015 / RP-003: Notification Log View
    Task<NotificationLogReportDto> GetNotificationLogAsync(
        NotificationLogFilterRequest filter,
        CancellationToken ct = default);

    // SF-014 / RP-001: Leave Summary Report (aggregate)
    Task<LeaveSummaryReportDto> GetLeaveSummaryAsync(
        LeaveHistoryFilterRequest filter,
        CancellationToken ct = default);
}
