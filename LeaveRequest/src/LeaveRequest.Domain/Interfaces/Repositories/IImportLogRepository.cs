namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface IImportLogRepository
{
    // Immutable: no Update / Delete
    Task AddAsync(ImportLog log, CancellationToken ct = default);

    // SFR-012: HR view import history
    Task<List<ImportLog>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
