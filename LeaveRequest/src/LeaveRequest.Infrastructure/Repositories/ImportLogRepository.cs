namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class ImportLogRepository(AppDbContext context) : IImportLogRepository
{
    public async Task AddAsync(ImportLog log, CancellationToken ct = default)
        => await context.ImportLogs.AddAsync(log, ct);

    public async Task<List<ImportLog>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        => await context.ImportLogs
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await context.ImportLogs.CountAsync(ct);
}
