namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface IHrisAdapter
{
    // IF-001 Pattern B — REST API pull (on-demand sync via IEmployeeService.SyncFromHrisAsync)
    Task<IEnumerable<HrisEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default);

    // IF-001 Pattern A — CSV batch file parse
    Task<IEnumerable<HrisEmployeeDto>> ParseBatchFileAsync(Stream csvStream, CancellationToken ct = default);
}
