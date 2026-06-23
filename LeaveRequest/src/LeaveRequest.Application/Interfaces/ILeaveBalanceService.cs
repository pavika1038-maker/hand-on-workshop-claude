namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface ILeaveBalanceService
{
    Task<LeaveBalanceDashboardDto> GetDashboardAsync(string employeeId, int year, CancellationToken ct = default);
}
