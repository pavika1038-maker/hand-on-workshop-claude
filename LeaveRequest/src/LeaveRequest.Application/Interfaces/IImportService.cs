namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface IImportService
{
    // IF-003: HR uploads .xlsx file to import Outsource employee records
    // SRS Trace: IF-003, SFR-012, SIR-003, VR-013, TR-006, BR-020
    Task<ImportResultDto> ImportOutsourceEmployeesAsync(
        string hrEmployeeId,
        Stream excelStream,
        string fileName,
        CancellationToken ct = default);
}
