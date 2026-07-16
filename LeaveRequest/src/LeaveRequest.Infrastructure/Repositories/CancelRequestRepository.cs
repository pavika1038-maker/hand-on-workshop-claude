namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CancelRequestRepository(AppDbContext context) : ICancelRequestRepository
{
    public async Task<CancelRequest?> GetByIdAsync(Guid cancelRequestId, CancellationToken ct = default)
        => await context.CancelRequests
            .Include(cr => cr.LeaveRequest)
            .FirstOrDefaultAsync(cr => cr.CancelRequestId == cancelRequestId && !cr.IsDeleted, ct);

    public async Task AddAsync(CancelRequest cancelRequest, CancellationToken ct = default)
        => await context.CancelRequests.AddAsync(cancelRequest, ct);

    // SF-013: cancel requests ที่ผูกกับคำขอลานี้ (audit timeline)
    public async Task<IReadOnlyList<CancelRequest>> GetByLeaveRequestAsync(
        Guid leaveRequestId, CancellationToken ct = default)
        => await context.CancelRequests.AsNoTracking()
            .Where(cr => cr.LeaveRequestId == leaveRequestId && !cr.IsDeleted)
            .OrderBy(cr => cr.CreatedAt)
            .ToListAsync(ct);

    // IF-005: only Pending records in the Reminder or Escalation window
    public async Task<IEnumerable<CancelRequest>> GetPendingForSlaCheckAsync(
        DateTime checkTime, CancellationToken ct = default)
    {
        // Reminder window: SlaDeadline - now ≤ 4h  →  SlaDeadline ≤ now + 4h
        // Escalation: SlaDeadline ≤ now  (already included in above)
        var reminderCutoff = checkTime.AddHours(4);
        return await context.CancelRequests
            .Include(cr => cr.LeaveRequest)
            .Where(cr =>
                cr.Status == CancelRequestStatus.Pending &&
                !cr.IsDeleted &&
                cr.SlaDeadline <= reminderCutoff)
            .ToListAsync(ct);
    }

    // Caller owns SaveChangesAsync — UpdateAsync just ensures EF tracks the entity
    public Task UpdateAsync(CancelRequest cancelRequest, CancellationToken ct = default)
    {
        context.CancelRequests.Update(cancelRequest);
        return Task.CompletedTask;
    }
}
