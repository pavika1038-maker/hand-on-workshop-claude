namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;

public sealed class ApprovalHistoryRepository(AppDbContext context) : IApprovalHistoryRepository
{
    public async Task AddAsync(ApprovalHistory history, CancellationToken ct = default)
        => await context.ApprovalHistories.AddAsync(history, ct);
}
