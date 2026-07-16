namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/reports")]
// [Authorize(Policy = "HrOnly")] — HR role only; enable after JWT auth is configured (SFR-015, NFR-005)
public class ReportsController : ControllerBase
{
    private readonly ILeaveReportService _service;

    public ReportsController(ILeaveReportService service)
    {
        _service = service;
    }

    /// <summary>รายงานประวัติการลาทั้งองค์กร — HR only (RPT-001)</summary>
    [HttpGet("leave-history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LeaveHistoryItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetLeaveHistory(
        [FromQuery] LeaveHistoryFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _service.GetLeaveHistoryAsync(filter, ct);

        var response = ApiResponse<PagedResult<LeaveHistoryItemDto>>.Ok(result);
        response.Metadata = new PaginationMeta
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };

        return Ok(response);
    }

    /// <summary>รายงานสรุปการลา (aggregate) — HR only (SF-014 / RP-001)</summary>
    [HttpGet("leave-summary")]
    [ProducesResponseType(typeof(ApiResponse<LeaveSummaryReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveSummary(
        [FromQuery] LeaveHistoryFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _service.GetLeaveSummaryAsync(filter, ct);
        return Ok(ApiResponse<LeaveSummaryReportDto>.Ok(result));
    }

    /// <summary>Notification Log View — HR only (SF-015 / RP-003)</summary>
    [HttpGet("notification-log")]
    [ProducesResponseType(typeof(ApiResponse<NotificationLogReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationLog(
        [FromQuery] NotificationLogFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _service.GetNotificationLogAsync(filter, ct);
        var response = ApiResponse<NotificationLogReportDto>.Ok(result);
        response.Metadata = new PaginationMeta
        {
            Page = result.Items.Page,
            PageSize = result.Items.PageSize,
            TotalCount = result.Items.TotalCount
        };
        return Ok(response);
    }
}
