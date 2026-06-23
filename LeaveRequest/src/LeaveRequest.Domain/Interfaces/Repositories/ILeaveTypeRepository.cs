namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface ILeaveTypeRepository
{
    Task<IReadOnlyList<LeaveType>> GetAllAsync(CancellationToken ct = default);
    Task<LeaveType?> GetByIdAsync(byte id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string typeCode, byte? excludeId = null, CancellationToken ct = default);
    Task<LeaveType> AddAsync(LeaveType leaveType, CancellationToken ct = default);
    void Update(LeaveType leaveType);
    void Delete(LeaveType leaveType);
}
