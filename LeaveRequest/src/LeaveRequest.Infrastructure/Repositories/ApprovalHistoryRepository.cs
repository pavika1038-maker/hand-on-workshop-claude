namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class ApprovalHistoryRepository(AppDbContext context) : IApprovalHistoryRepository
{
    public async Task AddAsync(ApprovalHistory history, CancellationToken ct = default)
        => await context.ApprovalHistories.AddAsync(history, ct);

    public async Task<IReadOnlyList<ApprovalHistory>> GetByLeaveRequestAsync(
        Guid leaveRequestId, CancellationToken ct = default)
        => await context.ApprovalHistories.AsNoTracking()
            .Where(h => h.LeaveRequestId == leaveRequestId)
            .OrderBy(h => h.ActionAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ApprovalHistory>> GetByCancelRequestIdsAsync(
        IReadOnlyCollection<Guid> cancelRequestIds, CancellationToken ct = default)
    {
        if (cancelRequestIds.Count == 0) return [];
        return await context.ApprovalHistories.AsNoTracking()
            .Where(h => h.CancelRequestId != null && cancelRequestIds.Contains(h.CancelRequestId.Value))
            .OrderBy(h => h.ActionAt)
            .ToListAsync(ct);
    }
}
