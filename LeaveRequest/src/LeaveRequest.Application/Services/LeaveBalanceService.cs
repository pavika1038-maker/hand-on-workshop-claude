namespace LeaveRequest.Application.Services;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;

public class LeaveBalanceService(
    ILeaveBalanceRepository repo,
    IEmployeeRepository employeeRepo) : ILeaveBalanceService
{
    public async Task<LeaveBalanceDashboardDto> GetDashboardAsync(
        string employeeId, int year, CancellationToken ct = default)
    {
        var balances = await repo.GetAllByEmployeeAsync(employeeId, year, ct);

        // BR-007: probation = อายุงาน < 3 เดือน — Regular นับจาก HireDate, Outsource นับจาก AbcStartDate
        var employee = await employeeRepo.GetByIdAsync(employeeId, ct);
        bool isProbation = false;
        if (employee != null)
        {
            var serviceStart = employee.EmployeeType == EmployeeType.Outsource
                ? (employee.AbcStartDate ?? employee.HireDate)
                : employee.HireDate;
            isProbation = serviceStart > DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-3);
        }

        return new LeaveBalanceDashboardDto(
            employeeId,
            year,
            isProbation,
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
