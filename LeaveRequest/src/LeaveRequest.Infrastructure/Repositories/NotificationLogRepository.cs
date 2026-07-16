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

    public async Task<(IReadOnlyList<NotificationLog> Items, int Total, int Success, int Failed)> GetForReportAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? eventType,
        string? recipient,
        DeliveryStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var q = _context.NotificationLogs.AsNoTracking().AsQueryable();

        // filter ตาม CreatedAt (SentAt เป็น null ได้เมื่อยังไม่ส่งสำเร็จ — ดู design §13)
        if (dateFrom.HasValue)
            q = q.Where(n => n.CreatedAt >= dateFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (dateTo.HasValue)
            q = q.Where(n => n.CreatedAt < dateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        if (!string.IsNullOrWhiteSpace(eventType))
            q = q.Where(n => n.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(recipient))
            q = q.Where(n => n.RecipientsJson.Contains(recipient));  // partial match ใน JSON
        if (status.HasValue)
            q = q.Where(n => n.Status == status.Value);

        var total   = await q.CountAsync(ct);
        var success = await q.CountAsync(n => n.Status == DeliveryStatus.Success, ct);
        var failed  = await q.CountAsync(n => n.Status == DeliveryStatus.Failed, ct);

        var items = await q
            .OrderByDescending(n => n.SentAt)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total, success, failed);
    }

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
