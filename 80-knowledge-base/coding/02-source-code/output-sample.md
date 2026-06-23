# Source Code Sample — SubmitLeaveRequest Use Case

> ตัวอย่าง source code สำหรับ use case "ยื่นคำร้องขอลา" (SFR-001)  
> อ้างอิงจาก: `leave-request-and-approval-method-signature.md`, `leave-request-and-approval-sequence-diagram.md`

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Use Case | ยื่นคำร้องขอลา (Submit Leave Request) |
| Function ID | SCR-001 |
| Related SRS | SFR-001, VR-001, VR-002, VR-003 |

---

## 2. Domain Layer

### 2.1 Entity: LeaveRequest.cs

```csharp
// File: src/LeaveRequest.Domain/Entities/LeaveRequest.cs
namespace LeaveRequest.Domain.Entities;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Draft;
    public string? AttachmentPath { get; set; }

    // Audit Columns
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public ICollection<LeaveApproval> Approvals { get; set; } = new List<LeaveApproval>();
}
```

### 2.2 Enum: LeaveStatus.cs

```csharp
// File: src/LeaveRequest.Domain/Enums/LeaveStatus.cs
namespace LeaveRequest.Domain.Enums;

public enum LeaveStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    Cancelled
}
```

### 2.3 Interface: ILeaveRequestRepository.cs

```csharp
// File: src/LeaveRequest.Domain/Interfaces/Repositories/ILeaveRequestRepository.cs
namespace LeaveRequest.Domain.Interfaces.Repositories;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetPendingApprovalAsync(int managerId, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
}
```

### 2.4 Interface: ILeaveService.cs

```csharp
// File: src/LeaveRequest.Domain/Interfaces/Services/ILeaveService.cs
namespace LeaveRequest.Domain.Interfaces.Services;

public interface ILeaveService
{
    Task<LeaveRequestResponse> SubmitLeaveRequestAsync(
        CreateLeaveRequest request,
        string submittedBy,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestResponse?> GetLeaveRequestByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LeaveRequestResponse>> GetMyLeaveRequestsAsync(
        int employeeId,
        CancellationToken cancellationToken = default);

    Task CancelLeaveRequestAsync(
        int id,
        string cancelledBy,
        CancellationToken cancellationToken = default);
}
```

---

## 3. Application Layer

### 3.1 DTO: LeaveRequestDTOs.cs

```csharp
// File: src/LeaveRequest.Application/DTOs/LeaveRequestDTOs.cs
namespace LeaveRequest.Application.DTOs;

public record CreateLeaveRequest(
    int EmployeeId,
    int LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalDays,
    string Reason,
    string? AttachmentPath = null
);

public record LeaveRequestResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    int LeaveTypeId,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalDays,
    string Status,
    string Reason,
    DateTime CreatedAt
);
```

### 3.2 Validator: CreateLeaveRequestValidator.cs

```csharp
// File: src/LeaveRequest.Application/Validators/CreateLeaveRequestValidator.cs
using FluentValidation;

namespace LeaveRequest.Application.Validators;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("EmployeeId ไม่ถูกต้อง");

        RuleFor(x => x.LeaveTypeId)
            .GreaterThan(0)
            .WithMessage("กรุณาเลือกประเภทการลา");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("วันที่เริ่มลาต้องไม่เป็นอดีต");    // VR-001

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("วันสิ้นสุดต้องไม่ก่อนวันเริ่มลา");

        RuleFor(x => x.TotalDays)
            .GreaterThan(0)
            .WithMessage("จำนวนวันลาต้องมากกว่า 0");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("กรุณาระบุเหตุผลการลา")
            .MaximumLength(500)
            .WithMessage("เหตุผลต้องไม่เกิน 500 ตัวอักษร");
    }
}
```

### 3.3 Service: LeaveService.cs (Submit method)

```csharp
// File: src/LeaveRequest.Application/Services/LeaveService.cs
namespace LeaveRequest.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRequestRepository _leaveRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(
        ILeaveRequestRepository leaveRepository,
        ILeaveBalanceRepository balanceRepository,
        IEmployeeRepository employeeRepository,
        AppDbContext context,
        ILogger<LeaveService> logger)
    {
        _leaveRepository = leaveRepository;
        _balanceRepository = balanceRepository;
        _employeeRepository = employeeRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<LeaveRequestResponse> SubmitLeaveRequestAsync(
        CreateLeaveRequest request,
        string submittedBy,
        CancellationToken cancellationToken = default)
    {
        // VR-002: ตรวจสอบวันลาคงเหลือ
        var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(
            request.EmployeeId, request.LeaveTypeId, DateTime.UtcNow.Year, cancellationToken);

        if (balance == null)
            throw new BusinessException("ไม่พบข้อมูลวันลาของพนักงาน", "VR-002-A");

        if (balance.RemainingDays < request.TotalDays)
            throw new BusinessException(
                $"วันลาคงเหลือไม่เพียงพอ (คงเหลือ {balance.RemainingDays} วัน แต่ขอ {request.TotalDays} วัน)",
                "VR-002");

        // VR-003: ตรวจสอบไม่มีคำร้องซ้ำในช่วงวันเดียวกัน
        var overlapping = await _leaveRepository.GetOverlappingAsync(
            request.EmployeeId, request.StartDate, request.EndDate, cancellationToken);

        if (overlapping.Any())
            throw new BusinessException("มีคำร้องขอลาในช่วงวันเดียวกันอยู่แล้ว", "VR-003");

        // Build entity
        var leaveRequest = new Domain.Entities.LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = request.TotalDays,
            Reason = request.Reason,
            AttachmentPath = request.AttachmentPath,
            Status = LeaveStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = submittedBy
        };

        // Persist with transaction
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _leaveRepository.AddAsync(leaveRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Leave request {Id} submitted by employee {EmployeeId} ({User})",
                leaveRequest.Id, request.EmployeeId, submittedBy);

            // Load navigation properties for response
            var created = await _leaveRepository.GetByIdAsync(leaveRequest.Id, cancellationToken)
                ?? throw new InvalidOperationException("Failed to reload created entity");

            return MapToResponse(created);
        }
        catch (BusinessException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex,
                "Failed to submit leave request for employee {EmployeeId}", request.EmployeeId);
            throw;
        }
    }

    public async Task<LeaveRequestResponse?> GetLeaveRequestByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _leaveRepository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToResponse(entity);
    }

    public async Task<IEnumerable<LeaveRequestResponse>> GetMyLeaveRequestsAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _leaveRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return entities.Select(MapToResponse);
    }

    public async Task CancelLeaveRequestAsync(
        int id,
        string cancelledBy,
        CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.LeaveRequest), id);

        if (leaveRequest.Status != LeaveStatus.PendingApproval && leaveRequest.Status != LeaveStatus.Draft)
            throw new BusinessException("ไม่สามารถยกเลิกคำร้องที่อนุมัติหรือปฏิเสธแล้ว", "BR-CANCEL-001");

        leaveRequest.Status = LeaveStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedBy = cancelledBy;

        await _leaveRepository.UpdateAsync(leaveRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Leave request {Id} cancelled by {User}", id, cancelledBy);
    }

    private static LeaveRequestResponse MapToResponse(Domain.Entities.LeaveRequest entity) => new(
        entity.Id,
        entity.EmployeeId,
        $"{entity.Employee?.FirstName} {entity.Employee?.LastName}",
        entity.LeaveTypeId,
        entity.LeaveType?.Name ?? string.Empty,
        entity.StartDate,
        entity.EndDate,
        entity.TotalDays,
        entity.Status.ToString(),
        entity.Reason,
        entity.CreatedAt
    );
}
```

---

## 4. Infrastructure Layer

### 4.1 Repository: LeaveRequestRepository.cs

```csharp
// File: src/LeaveRequest.Infrastructure/Repositories/LeaveRequestRepository.cs
using Microsoft.EntityFrameworkCore;

namespace LeaveRequest.Infrastructure.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly AppDbContext _context;

    public LeaveRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.Approvals)
                .ThenInclude(a => a.Approver)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Include(x => x.LeaveType)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingApprovalAsync(
        int managerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Where(x => x.Status == LeaveStatus.PendingApproval
                     && x.Employee.ManagerId == managerId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetOverlappingAsync(
        int employeeId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Where(x => x.EmployeeId == employeeId
                     && x.Status != LeaveStatus.Cancelled
                     && x.Status != LeaveStatus.Rejected
                     && x.StartDate <= endDate
                     && x.EndDate >= startDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        await _context.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        _context.LeaveRequests.Update(leaveRequest);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        _context.LeaveRequests.Remove(leaveRequest);
        await Task.CompletedTask;
    }
}
```

---

## 5. API Layer

### 5.1 Controller: LeaveRequestsController.cs

```csharp
// File: src/LeaveRequest.API/Controllers/LeaveRequestsController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveRequest.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly ILogger<LeaveRequestsController> _logger;

    public LeaveRequestsController(ILeaveService leaveService, ILogger<LeaveRequestsController> logger)
    {
        _leaveService = leaveService;
        _logger = logger;
    }

    /// <summary>ยื่นคำร้องขอลา</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var submittedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await _leaveService.SubmitLeaveRequestAsync(request, submittedBy, cancellationToken);
        return CreatedAtAction(nameof(GetLeaveRequest), new { id = result.Id },
            ApiResponse<LeaveRequestResponse>.Success(result));
    }

    /// <summary>ดูรายละเอียดคำร้องขอลา</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetLeaveRequestByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(LeaveRequest), id);
        return Ok(ApiResponse<LeaveRequestResponse>.Success(result));
    }

    /// <summary>ดูรายการคำร้องขอลาของฉัน</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLeaveRequests(CancellationToken cancellationToken)
    {
        var employeeId = int.Parse(User.FindFirst("EmployeeId")?.Value ?? "0");
        var results = await _leaveService.GetMyLeaveRequestsAsync(employeeId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<LeaveRequestResponse>>.Success(results));
    }

    /// <summary>ยกเลิกคำร้องขอลา</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelLeaveRequest(int id, CancellationToken cancellationToken)
    {
        var cancelledBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _leaveService.CancelLeaveRequestAsync(id, cancelledBy, cancellationToken);
        return NoContent();
    }
}
```

---

## 6. Program.cs Registration

```csharp
// ส่วนที่เพิ่มใน Program.cs
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateLeaveRequestValidator>();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

## 7. Deviation Log

| # | Item | Expected (Design) | Actual (Code) | Reason | Status |
|---|------|------------------|---------------|--------|--------|
| 1 | VR-002 check | ตรวจ balance ก่อน save | Implemented ก่อน BeginTransaction | เพื่อ fail fast ก่อน open transaction | Accepted |
| 2 | GetOverlapping method | ไม่ได้ระบุใน method signature | เพิ่มใน repository | จำเป็นสำหรับ VR-003 | Accepted |
