namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface IApprovalHistoryRepository
{
    Task AddAsync(ApprovalHistory history, CancellationToken ct = default);
}
