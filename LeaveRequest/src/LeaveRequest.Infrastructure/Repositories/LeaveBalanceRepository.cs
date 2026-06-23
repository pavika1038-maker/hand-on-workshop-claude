namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class LeaveBalanceRepository(AppDbContext context) : ILeaveBalanceRepository
{
    public async Task<LeaveBalance?> GetAsync(
        string employeeId,
        byte leaveTypeId,
        int year,
        CancellationToken ct = default)
        => await context.LeaveBalances
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId &&
                x.LeaveTypeId == leaveTypeId &&
                x.LeaveYear == year, ct);

    public async Task<IReadOnlyList<LeaveBalance>> GetAllByEmployeeAsync(
        string employeeId,
        int year,
        CancellationToken ct = default)
        => await context.LeaveBalances
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId && x.LeaveYear == year)
            .OrderBy(x => x.LeaveTypeId)
            .ToListAsync(ct);

    public void Update(LeaveBalance leaveBalance)
        => context.LeaveBalances.Update(leaveBalance);
}
