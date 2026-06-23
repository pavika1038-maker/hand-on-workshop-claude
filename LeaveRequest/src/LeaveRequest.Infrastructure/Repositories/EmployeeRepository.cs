namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(string employeeId, CancellationToken ct = default)
        => await context.Employees
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, ct);

    // IF-003: one DB round-trip to load all active emails for duplicate check
    public async Task<HashSet<string>> GetAllEmailsAsync(CancellationToken ct = default)
    {
        var emails = await context.Employees
            .Select(x => x.Email)
            .ToListAsync(ct);
        return new HashSet<string>(emails, StringComparer.OrdinalIgnoreCase);
    }

    // IF-003: insert new or update existing outsource employee (no SaveChanges here)
    public async Task UpsertAsync(Employee employee, CancellationToken ct = default)
    {
        var existing = await context.Employees
            .IgnoreQueryFilters()  // bypass soft-delete global filter
            .FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId, ct);

        if (existing is null)
        {
            await context.Employees.AddAsync(employee, ct);
        }
        else
        {
            existing.EmployeeCode  = employee.EmployeeCode;
            existing.FullNameTh    = employee.FullNameTh;
            existing.FullNameEn    = employee.FullNameEn;
            existing.Email         = employee.Email;
            existing.Department    = employee.Department;
            existing.Position      = employee.Position;
            existing.AgencyCompany = employee.AgencyCompany;
            existing.AbcStartDate  = employee.AbcStartDate;
            existing.ManagerId     = employee.ManagerId;
            existing.IsActive      = true;
            existing.IsDeleted     = false;
            existing.UpdatedAt     = employee.UpdatedAt;
            existing.UpdatedBy     = employee.UpdatedBy;
        }
    }
}
