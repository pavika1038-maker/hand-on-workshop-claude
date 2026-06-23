namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;

public interface INotificationLogRepository
{
    Task<NotificationLog?> GetByIdAsync(Guid notificationLogId, CancellationToken ct = default);

    // IF-002 idempotency check: returns true when Status == Success (already delivered)
    Task<bool> ExistsAsync(Guid notificationLogId, CancellationToken ct = default);

    Task AddAsync(NotificationLog log, CancellationToken ct = default);

    Task UpdateDeliveryStatusAsync(
        Guid notificationLogId,
        DeliveryStatus status,
        int retryCount,
        DateTime? sentAt,
        string? failureReason,
        CancellationToken ct = default);
}
