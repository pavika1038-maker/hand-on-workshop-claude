namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class LeaveTypeRepository(AppDbContext context) : ILeaveTypeRepository
{
    public async Task<IReadOnlyList<LeaveType>> GetAllAsync(CancellationToken ct = default)
        => await context.LeaveTypes
            .OrderBy(x => x.LeaveTypeId)
            .ToListAsync(ct);

    public async Task<LeaveType?> GetByIdAsync(byte id, CancellationToken ct = default)
        => await context.LeaveTypes
            .FirstOrDefaultAsync(x => x.LeaveTypeId == id, ct);

    public async Task<bool> ExistsByCodeAsync(string typeCode, byte? excludeId = null, CancellationToken ct = default)
        => await context.LeaveTypes
            .AnyAsync(x => x.TypeCode == typeCode
                        && (excludeId == null || x.LeaveTypeId != excludeId), ct);

    public async Task<LeaveType> AddAsync(LeaveType leaveType, CancellationToken ct = default)
    {
        await context.LeaveTypes.AddAsync(leaveType, ct);
        return leaveType;
    }

    public void Update(LeaveType leaveType)
        => context.LeaveTypes.Update(leaveType);

    public void Delete(LeaveType leaveType)
        => context.LeaveTypes.Remove(leaveType);
}
