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
}
