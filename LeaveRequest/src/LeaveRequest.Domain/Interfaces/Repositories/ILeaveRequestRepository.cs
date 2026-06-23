namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid leaveRequestId, CancellationToken ct = default);

    Task<IEnumerable<LeaveRequest>> GetOverlappingAsync(
        string employeeId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default);

    /// <summary>รายการคำร้องของพนักงาน (SCR-003 My List, SCR-005 History)</summary>
    Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetByEmployeeAsync(
        string employeeId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>รายการรอ Manager อนุมัติ (SCR-004)</summary>
    Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetPendingByManagerAsync(
        string managerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>รายการ CancelRequest รอ Manager อนุมัติ (SCR-007)</summary>
    Task<(IReadOnlyList<CancelRequest> Items, int Total)> GetCancelRequestsByManagerAsync(
        string managerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>รายการทั้งหมดสำหรับ HR (SCR-008)</summary>
    Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetAllForHrAsync(
        string? status,
        string? department,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(LeaveRequest leaveRequest, CancellationToken ct = default);
    void Update(LeaveRequest leaveRequest);
}
