namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(string employeeId, CancellationToken ct = default);

    // IF-003: load existing email set for duplicate check (one DB round-trip)
    Task<HashSet<string>> GetAllEmailsAsync(CancellationToken ct = default);

    // IF-003: insert new or update existing outsource employee
    Task UpsertAsync(Employee employee, CancellationToken ct = default);
}
