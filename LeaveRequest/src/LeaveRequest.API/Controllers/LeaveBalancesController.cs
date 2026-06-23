namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/leave-balances")]
public class LeaveBalancesController(ILeaveBalanceService balanceService) : ControllerBase
{
    private string CallerEmployeeId =>
        Request.Headers["X-Employee-Id"].FirstOrDefault() ?? string.Empty;

    /// <summary>Leave balance dashboard สำหรับพนักงาน (SCR-002, SFR-002)</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] string? employeeId,
        [FromQuery] int? year,
        CancellationToken ct)
    {
        var eid = employeeId ?? CallerEmployeeId;
        if (string.IsNullOrWhiteSpace(eid))
            return BadRequest(ApiResponse<object>.Fail("MISSING_EMPLOYEE_ID", "ระบุ employeeId หรือส่ง X-Employee-Id header"));

        var dto = await balanceService.GetDashboardAsync(eid, year ?? DateTime.UtcNow.Year, ct);
        return Ok(ApiResponse<object>.Ok(dto));
    }
}
