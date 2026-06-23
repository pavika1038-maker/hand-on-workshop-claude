namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface ICancelRequestRepository
{
    Task<CancelRequest?> GetByIdAsync(Guid cancelRequestId, CancellationToken ct = default);
    Task AddAsync(CancelRequest cancelRequest, CancellationToken ct = default);

    // IF-005: returns Pending records in Reminder or Escalation window
    // Where: Status==Pending && !IsDeleted
    //     && (SlaDeadline.AddHours(-4) <= checkTime || SlaDeadline <= checkTime)
    Task<IEnumerable<CancelRequest>> GetPendingForSlaCheckAsync(
        DateTime checkTime, CancellationToken ct = default);

    // Caller owns SaveChangesAsync after calling this
    Task UpdateAsync(CancelRequest cancelRequest, CancellationToken ct = default);
}
