namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/approvals")]
public class ApprovalsController(ILeaveRequestService service) : ControllerBase
{
    private string CallerEmployeeId =>
        Request.Headers["X-Employee-Id"].FirstOrDefault() ?? string.Empty;

    /// <summary>รายการรอ Manager อนุมัติ (SCR-004)</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] string? managerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken ct = default)
    {
        var mid = managerId ?? CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(mid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_MANAGER_ID", "ระบุ managerId หรือส่ง X-Employee-Id header"));

        var result = await service.GetPendingByManagerAsync(mid, page, pageSize, ct);
        var response = ApiResponse<PagedResult<PendingApprovalDto>>.Ok(result);
        response.Metadata = new PaginationMeta { Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount };
        return Ok(response);
    }

    /// <summary>อนุมัติคำร้อง (SCR-004)</summary>
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApproveRejectDto body, CancellationToken ct)
    {
        var mid = CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(mid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_MANAGER_ID", "ส่ง X-Employee-Id header"));

        await service.ApproveAsync(id, mid, body.Comment, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "อนุมัติสำเร็จ" }));
    }

    /// <summary>ปฏิเสธคำร้อง (SCR-004)</summary>
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] ApproveRejectDto body, CancellationToken ct)
    {
        var mid = CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(mid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_MANAGER_ID", "ส่ง X-Employee-Id header"));

        await service.RejectAsync(id, mid, body.Comment, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "ปฏิเสธสำเร็จ" }));
    }

    /// <summary>รายการ CancelRequest รอ Manager อนุมัติ (SCR-007)</summary>
    [HttpGet("cancel-requests")]
    public async Task<IActionResult> GetCancelRequests(
        [FromQuery] string? managerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken ct = default)
    {
        var mid = managerId ?? CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(mid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_MANAGER_ID", "ระบุ managerId หรือส่ง X-Employee-Id header"));

        var result = await service.GetCancelRequestsByManagerAsync(mid, page, pageSize, ct);
        var response = ApiResponse<PagedResult<PendingCancelRequestDto>>.Ok(result);
        response.Metadata = new PaginationMeta { Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount };
        return Ok(response);
    }

    /// <summary>อนุมัติ CancelRequest (SCR-007)</summary>
    [HttpPatch("cancel-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveCancel(
        Guid id, [FromBody] ApproveRejectDto body, CancellationToken ct)
    {
        var mid = CallerEmployeeId;
        await service.ApproveCancelAsync(id, mid, body.Comment, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "อนุมัติการยกเลิกสำเร็จ" }));
    }

    /// <summary>ปฏิเสธ CancelRequest (SCR-007)</summary>
    [HttpPatch("cancel-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectCancel(
        Guid id, [FromBody] ApproveRejectDto body, CancellationToken ct)
    {
        var mid = CallerEmployeeId;
        await service.RejectCancelAsync(id, mid, body.Comment, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "ปฏิเสธการยกเลิกสำเร็จ" }));
    }
}
