namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/hr")]
public class HrController(IImportService importService, ILeaveRequestService leaveService, IImportLogRepository importLogRepo) : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const string AllowedContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private string CallerEmployeeId =>
        Request.Headers["X-Employee-Id"].FirstOrDefault() ?? string.Empty;

    /// <summary>IF-003: HR upload Outsource employee Excel (SCR-009, SFR-012)</summary>
    [HttpPost("outsource-imports")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadOutsourceImport(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("IMPORT_NO_FILE", "กรุณาเลือกไฟล์ Excel (.xlsx) ก่อน upload"));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse<object>.Fail("IMPORT_FILE_TOO_LARGE",
                $"ขนาดไฟล์ ({file.Length / (1024 * 1024.0):F1} MB) เกิน 10 MB ที่กำหนด"));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" || file.ContentType != AllowedContentType)
            return BadRequest(ApiResponse<object>.Fail("IMPORT_WRONG_FILE_TYPE",
                "รองรับเฉพาะไฟล์ .xlsx เท่านั้น"));

        var hrEmployeeId = CallerEmployeeId.Length > 0 ? CallerEmployeeId : "SYSTEM";
        await using var stream = file.OpenReadStream();
        var result = await importService.ImportOutsourceEmployeesAsync(hrEmployeeId, stream, file.FileName, ct);

        if (result.IsRolledBack)
            return UnprocessableEntity(ApiResponse<ImportResultDto>.Fail("IMPORT_ROLLBACK",
                $"นำเข้าไม่สำเร็จ: error {result.FailedRecords}/{result.TotalRecords} แถว — ยกเลิกทั้งหมด",
                result.Errors.Select(e => $"Row {e.RowNumber} [{e.Field}]: {e.Message}")));

        return Ok(ApiResponse<ImportResultDto>.Ok(result));
    }

    /// <summary>HR ดูประวัติ import (SCR-009, SFR-012)</summary>
    [HttpGet("outsource-imports")]
    public async Task<IActionResult> GetImportHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var items      = await importLogRepo.GetAllAsync(page, pageSize, ct);
        var totalCount = await importLogRepo.CountAsync(ct);

        var result = new PagedResult<ImportLogSummaryDto>
        {
            Items      = items.Select(x => new ImportLogSummaryDto(
                x.ImportLogId, x.FileName, x.ImportedBy,
                x.TotalRecords, x.SuccessRecords, x.FailedRecords,
                x.IsRolledBack, x.CreatedAt)).ToList(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
        };

        var response = ApiResponse<PagedResult<ImportLogSummaryDto>>.Ok(result);
        response.Metadata = new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = totalCount };
        return Ok(response);
    }

    /// <summary>HR ดูรายการลาทั้งองค์กร (SCR-008, SFR-011)</summary>
    [HttpGet("leave-requests")]
    public async Task<IActionResult> GetAllLeaveRequests(
        [FromQuery] string? status,
        [FromQuery] string? department,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await leaveService.GetAllForHrAsync(status, department, page, pageSize, ct);
        var response = ApiResponse<PagedResult<HrLeaveRequestDto>>.Ok(result);
        response.Metadata = new PaginationMeta
        {
            Page       = result.Page,
            PageSize   = result.PageSize,
            TotalCount = result.TotalCount
        };
        return Ok(response);
    }
}
