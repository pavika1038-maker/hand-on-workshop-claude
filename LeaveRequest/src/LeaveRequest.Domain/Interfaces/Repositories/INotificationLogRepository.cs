namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;

public interface INotificationLogRepository
{
    Task<NotificationLog?> GetByIdAsync(Guid notificationLogId, CancellationToken ct = default);

    // IF-002 idempotency check: returns true when Status == Success (already delivered)
    Task<bool> ExistsAsync(Guid notificationLogId, CancellationToken ct = default);

    // SF-015 / RP-003: filtered log สำหรับ HR + counts สำหรับ summary strip
    Task<(IReadOnlyList<NotificationLog> Items, int Total, int Success, int Failed)> GetForReportAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? eventType,
        string? recipient,
        DeliveryStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(NotificationLog log, CancellationToken ct = default);

    Task UpdateDeliveryStatusAsync(
        Guid notificationLogId,
        DeliveryStatus status,
        int retryCount,
        DateTime? sentAt,
        string? failureReason,
        CancellationToken ct = default);
}
