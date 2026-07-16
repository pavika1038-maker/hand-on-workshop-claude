namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface IApprovalHistoryRepository
{
    Task AddAsync(ApprovalHistory history, CancellationToken ct = default);

    // SF-013: audit trail timeline
    Task<IReadOnlyList<ApprovalHistory>> GetByLeaveRequestAsync(Guid leaveRequestId, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalHistory>> GetByCancelRequestIdsAsync(IReadOnlyCollection<Guid> cancelRequestIds, CancellationToken ct = default);
}
