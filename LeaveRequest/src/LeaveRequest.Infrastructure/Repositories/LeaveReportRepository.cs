namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeaveRequestEntity = global::LeaveRequest.Domain.Entities.LeaveRequest;

public class LeaveReportRepository : ILeaveReportRepository
{
    private readonly AppDbContext _context;

    public LeaveReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<LeaveRequestEntity> Items, int TotalCount)> GetLeaveHistoryAsync(
        LeaveHistoryQuery query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var q = _context.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        if (query.StartDate.HasValue)
            q = q.Where(lr => lr.EndDate >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            q = q.Where(lr => lr.StartDate <= query.EndDate.Value);

        if (query.EmployeeId is not null)
            q = q.Where(lr => lr.EmployeeId == query.EmployeeId);

        if (query.LeaveTypeId.HasValue)
            q = q.Where(lr => lr.LeaveTypeId == query.LeaveTypeId.Value);

        if (query.Status.HasValue)
            q = q.Where(lr => lr.Status == query.Status.Value);

        if (query.Department is not null)
            q = q.Where(lr => lr.Employee.Department == query.Department);

        if (query.EmployeeType.HasValue)
            q = q.Where(lr => lr.Employee.EmployeeType == query.EmployeeType.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
