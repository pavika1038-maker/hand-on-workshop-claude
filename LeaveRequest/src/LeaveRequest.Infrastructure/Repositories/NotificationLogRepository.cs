namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppDbContext _context;

    public NotificationLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationLog?> GetByIdAsync(Guid notificationLogId, CancellationToken ct = default) =>
        await _context.NotificationLogs
            .FirstOrDefaultAsync(n => n.NotificationLogId == notificationLogId, ct);

    public async Task<bool> ExistsAsync(Guid notificationLogId, CancellationToken ct = default) =>
        await _context.NotificationLogs
            .AnyAsync(n => n.NotificationLogId == notificationLogId
                        && n.Status == DeliveryStatus.Success, ct);

    public async Task AddAsync(NotificationLog log, CancellationToken ct = default)
    {
        await _context.NotificationLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateDeliveryStatusAsync(
        Guid notificationLogId,
        DeliveryStatus status,
        int retryCount,
        DateTime? sentAt,
        string? failureReason,
        CancellationToken ct = default)
    {
        await _context.NotificationLogs
            .Where(n => n.NotificationLogId == notificationLogId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.Status, status)
                .SetProperty(n => n.RetryCount, retryCount)
                .SetProperty(n => n.SentAt, sentAt)
                .SetProperty(n => n.FailureReason, failureReason),
                ct);
    }
}
