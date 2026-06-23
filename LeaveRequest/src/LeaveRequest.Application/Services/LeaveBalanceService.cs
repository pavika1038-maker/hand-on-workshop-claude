namespace LeaveRequest.Application.Services;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;

public class LeaveBalanceService(ILeaveBalanceRepository repo) : ILeaveBalanceService
{
    public async Task<LeaveBalanceDashboardDto> GetDashboardAsync(
        string employeeId, int year, CancellationToken ct = default)
    {
        var balances = await repo.GetAllByEmployeeAsync(employeeId, year, ct);
        return new LeaveBalanceDashboardDto(
            employeeId,
            year,
            balances.Select(b => new LeaveBalanceItemDto(
                b.LeaveTypeId,
                b.LeaveType.TypeCode,
                b.LeaveType.TypeNameTh,
                b.EntitledDays,
                b.UsedDays,
                b.PendingDays,
                b.CarriedForwardDays,
                b.RemainingDays
            )).ToList()
        );
    }
}
