namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/leave-requests")]
public class LeaveRequestsController(ILeaveRequestService service) : ControllerBase
{
    // helper: อ่าน employeeId จาก X-Employee-Id header (mock auth)
    private string CallerEmployeeId =>
        Request.Headers["X-Employee-Id"].FirstOrDefault() ?? string.Empty;

    /// <summary>รายการคำร้องของฉัน (SCR-003 list)</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] string? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var eid = employeeId ?? CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(eid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_EMPLOYEE_ID", "ระบุ employeeId หรือส่ง X-Employee-Id header"));

        var result = await service.GetMyRequestsAsync(eid, page, pageSize, ct);
        var response = ApiResponse<PagedResult<LeaveRequestSummaryDto>>.Ok(result);
        response.Metadata = new PaginationMeta { Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount };
        return Ok(response);
    }

    /// <summary>รายละเอียดคำร้อง (SCR-005)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLeaveRequestById(Guid id, CancellationToken ct)
    {
        var result = await service.GetDetailAsync(id, ct);
        return Ok(ApiResponse<LeaveRequestDetailDto>.Ok(result));
    }

    /// <summary>Audit trail timeline ของคำร้อง (SCR-005, SF-013)</summary>
    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var result = await service.GetTimelineAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<TimelineEventDto>>.Ok(result));
    }

    /// <summary>ยื่นคำร้องขอลา (SCR-003 submit, SFR-003)</summary>
    [HttpPost]
    public async Task<IActionResult> SubmitLeaveRequest(
        [FromBody] CreateLeaveRequestDto request, CancellationToken ct)
    {
        var eid = CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(eid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_EMPLOYEE_ID", "ส่ง X-Employee-Id header พร้อม request"));

        var result = await service.SubmitLeaveRequestAsync(eid, request, ct);
        return CreatedAtAction(nameof(GetLeaveRequestById), new { id = result.LeaveRequestId },
            ApiResponse<LeaveRequestResult>.Ok(result));
    }

    /// <summary>ยกเลิกคำร้อง (SCR-006)</summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeaveRequest(
        Guid id,
        [FromBody] ApproveRejectDto body,
        CancellationToken ct)
    {
        var eid = CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(eid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_EMPLOYEE_ID", "ส่ง X-Employee-Id header"));

        var msg = await service.CancelAsync(id, eid, body.Comment, ct);
        return Ok(ApiResponse<object>.Ok(new { message = msg }));
    }
}
