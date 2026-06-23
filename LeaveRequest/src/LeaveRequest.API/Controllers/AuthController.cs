namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string EmployeeId, string FullNameTh, string Email, string Role);

[ApiController]
[Route("api/v1/auth")]
public class AuthController(AppDbContext db) : ControllerBase
{
    private const string MockPassword = "1234";

    // Role mapping: HR Dept = HR, Manager position = Manager, else Employee
    private static string ResolveRole(string? department, string? position) =>
        department?.ToUpper() == "HR" ? "HR"
        : position?.ToLower().Contains("manager") == true ? "Manager"
        : "Employee";

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (request.Password != MockPassword)
            return Unauthorized(ApiResponse<object>.Fail("INVALID_CREDENTIALS", "Username หรือ Password ไม่ถูกต้อง"));

        // username = email
        var employee = await db.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Email == request.Username && e.IsActive && !e.IsDeleted, ct);

        if (employee is null)
            return Unauthorized(ApiResponse<object>.Fail("INVALID_CREDENTIALS", "Username หรือ Password ไม่ถูกต้อง"));

        var role = ResolveRole(employee.Department, employee.Position);
        var response = new LoginResponse(employee.EmployeeId, employee.FullNameTh, employee.Email, role);

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }
}
