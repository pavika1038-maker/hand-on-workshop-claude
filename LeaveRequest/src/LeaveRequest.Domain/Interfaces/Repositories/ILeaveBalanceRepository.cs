namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(
        string employeeId,
        byte leaveTypeId,
        int year,
        CancellationToken ct = default);

    /// <summary>ดึงสิทธิ์วันลาทุกประเภทของพนักงานในปีที่ระบุ (SCR-002)</summary>
    Task<IReadOnlyList<LeaveBalance>> GetAllByEmployeeAsync(
        string employeeId,
        int year,
        CancellationToken ct = default);

    void Update(LeaveBalance leaveBalance);
}
