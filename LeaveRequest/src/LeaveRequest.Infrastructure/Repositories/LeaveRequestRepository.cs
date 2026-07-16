namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class LeaveRequestRepository(AppDbContext context) : ILeaveRequestRepository
{
    public async Task<LeaveRequest?> GetByIdAsync(Guid leaveRequestId, CancellationToken ct = default)
        => await context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId, ct);

    public async Task<IEnumerable<LeaveRequest>> GetOverlappingAsync(
        string employeeId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
        => await context.LeaveRequests
            .Where(x => x.EmployeeId == employeeId
                     && (x.Status == LeaveStatus.Pending || x.Status == LeaveStatus.Approved)
                     && x.StartDate <= endDate
                     && x.EndDate >= startDate)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetByEmployeeAsync(
        string employeeId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.LeaveRequests
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetPendingByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        // รายการ Status=Pending ของ employee ที่รายงาน manager คนนี้โดยตรง
        var query = context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => x.Status == LeaveStatus.Pending
                     && x.Employee.ManagerId == managerId)
            .OrderBy(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetProcessedByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        // SF-004: คำขอที่ Approve/Reject แล้วของ direct report — เรียงล่าสุดก่อน
        var query = context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => (x.Status == LeaveStatus.Approved || x.Status == LeaveStatus.Rejected)
                     && x.Employee.ManagerId == managerId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<CancelRequest> Items, int Total)> GetCancelRequestsByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        // CancelRequest ที่ pending และ LeaveRequest เป็นของ direct report
        var query = context.CancelRequests
            .Include(cr => cr.LeaveRequest)
                .ThenInclude(lr => lr!.Employee)
            .Include(cr => cr.LeaveRequest)
                .ThenInclude(lr => lr!.LeaveType)
            .Where(cr => cr.Status == CancelRequestStatus.Pending
                      && !cr.IsDeleted
                      && cr.LeaveRequest!.Employee.ManagerId == managerId)
            .OrderBy(cr => cr.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<LeaveRequest> Items, int Total)> GetAllForHrAsync(
        string? status, string? department, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<LeaveStatus>(status, ignoreCase: true, out var parsed))
            query = query.Where(x => x.Status == parsed);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(x => x.Employee.Department == department);

        query = query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(LeaveRequest leaveRequest, CancellationToken ct = default)
        => await context.LeaveRequests.AddAsync(leaveRequest, ct);

    public void Update(LeaveRequest leaveRequest)
        => context.LeaveRequests.Update(leaveRequest);
}
