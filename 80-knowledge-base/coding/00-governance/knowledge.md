# Coding Standard — ASP.NET Core C# + EF Core + SQLite

> มาตรฐานนี้ใช้สำหรับโปรเจคที่ใช้ .NET 8, ASP.NET Core Web API, Entity Framework Core และ SQLite  
> AI ต้องอ่านและยึดตามเอกสารนี้ก่อนสร้าง entity, repository, service, controller หรือ unit test ทุกครั้ง

---

## 1. Project Structure (Clean/Layered Architecture)

```
LeaveRequest/
├── LeaveRequest.sln
└── src/
    ├── LeaveRequest.Domain/               ← Entity, Enum, Interface (ไม่ depend กับ framework ใด)
    │   ├── Entities/
    │   ├── Enums/
    │   └── Interfaces/
    │       ├── Repositories/
    │       └── Services/
    ├── LeaveRequest.Application/          ← Service Implementation, DTO, Validation
    │   ├── DTOs/
    │   ├── Services/
    │   └── Validators/
    ├── LeaveRequest.Infrastructure/       ← DbContext, Repository Implementation, Migration
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   └── Migrations/
    │   └── Repositories/
    └── LeaveRequest.API/                  ← Controller, Middleware, Program.cs
        ├── Controllers/
        ├── Middleware/
        ├── ClientApp/                     ← React 18 + Vite
        └── Program.cs
```

**กฎหลัก:**
- `Domain` layer ห้าม reference `Infrastructure` หรือ `Application`
- `Application` reference ได้เฉพาะ `Domain`
- `Infrastructure` reference `Domain` และ `Application`
- `API` reference ได้ทุก layer แต่ business logic ต้องอยู่ใน `Application` เท่านั้น

---

## 2. Naming Convention

### 2.1 ชื่อ Class / Interface

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Entity | PascalCase (noun) | `LeaveRequest`, `Employee`, `Department` |
| Interface | `I` + PascalCase | `ILeaveRequestRepository`, `ILeaveService` |
| Repository (impl) | `{Entity}Repository` | `LeaveRequestRepository` |
| Service (impl) | `{Domain}Service` | `LeaveService`, `NotificationService` |
| Controller | `{Resource}Controller` | `LeaveRequestsController` |
| DTO (Request) | `{Action}{Entity}Request` | `CreateLeaveRequest`, `UpdateLeaveStatusRequest` |
| DTO (Response) | `{Entity}Response` | `LeaveRequestResponse`, `EmployeeResponse` |
| Enum | PascalCase | `LeaveStatus`, `LeaveType`, `ApprovalAction` |
| DbContext | `AppDbContext` | — |
| Validator | `{DTO}Validator` | `CreateLeaveRequestValidator` |

### 2.2 ชื่อ Method

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Repository อ่าน | `GetBy{Field}Async` / `GetAllAsync` | `GetByIdAsync`, `GetByEmployeeIdAsync` |
| Repository บันทึก | `AddAsync`, `UpdateAsync`, `DeleteAsync` | — |
| Service action | `{Verb}{Noun}Async` | `SubmitLeaveRequestAsync`, `ApproveLeaveAsync` |
| Controller action | `Get`, `Create`, `Update`, `Delete` | `GetLeaveRequests`, `CreateLeaveRequest` |

### 2.3 ชื่อ Property / Field / Variable

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Public property | PascalCase | `StartDate`, `EmployeeId`, `TotalDays` |
| Private field | `_camelCase` | `_repository`, `_dbContext`, `_logger` |
| Local variable | camelCase | `leaveRequest`, `employee`, `totalDays` |
| Parameter | camelCase | `employeeId`, `startDate`, `request` |
| Constant | PascalCase หรือ UPPER_SNAKE | `MaxLeaveDaysPerYear`, `MAX_RETRY_COUNT` |

### 2.4 ชื่อ Table / Column (EF Core → SQLite)

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Table name | PascalCase (plural) | `LeaveRequests`, `Employees`, `Departments` |
| Column name | PascalCase | `EmployeeId`, `StartDate`, `Status` |
| Primary key | `Id` | — |
| Foreign key | `{Entity}Id` | `EmployeeId`, `DepartmentId`, `ApproverId` |
| Audit columns | PascalCase | `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` |

### 2.5 ชื่อ API Endpoint (RESTful)

```
GET    /api/v1/leave-requests              ← ดึงรายการ
GET    /api/v1/leave-requests/{id}         ← ดึงตาม id
POST   /api/v1/leave-requests              ← สร้าง
PUT    /api/v1/leave-requests/{id}         ← แก้ไขทั้งหมด
PATCH  /api/v1/leave-requests/{id}/status  ← แก้ไขบางส่วน
DELETE /api/v1/leave-requests/{id}         ← ลบ
```

---

## 3. Entity Design (Domain Layer)

### 3.1 กฎ Entity

```csharp
// ✅ ถูกต้อง
public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Draft;

    // Audit columns — ต้องมีทุก entity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public ICollection<LeaveApproval> Approvals { get; set; } = new List<LeaveApproval>();
}
```

**กฎ:**
- ทุก Entity ต้องมี `Id` (int หรือ Guid) เป็น PK
- ทุก Entity ต้องมี audit columns: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- String property ที่ required ให้ default เป็น `string.Empty`
- Navigation property ให้ default เป็น `null!` (non-nullable reference type)
- ห้าม logic ใน Entity (ให้อยู่ใน Service)
- ใช้ `DateOnly` สำหรับวันที่ไม่มีเวลา, `DateTime` สำหรับ timestamp

### 3.2 EF Core Configuration

```csharp
// LeaveRequestConfiguration.cs (ใน Infrastructure/Data/Configurations/)
public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()  // เก็บ enum เป็น string ใน SQLite
            .HasMaxLength(50);

        builder.Property(x => x.TotalDays)
            .HasColumnType("REAL");   // SQLite ใช้ REAL สำหรับ decimal

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

**กฎ EF Core + SQLite:**
- ใช้ `HasColumnType("REAL")` สำหรับ `decimal` (SQLite ไม่มี DECIMAL native)
- ใช้ `HasConversion<string>()` สำหรับ enum เพื่อ readability
- ใช้ Fluent API ใน Configuration class แยกไฟล์ (ห้าม Data Annotation บน Entity)
- `OnDelete(DeleteBehavior.Restrict)` เป็น default — ห้าม Cascade ยกเว้นมีเหตุผลชัดเจน
- ห้ามใช้ SQL Server-specific syntax เช่น `NVARCHAR`, `UNIQUEIDENTIFIER`, `GETDATE()`

---

## 4. Repository Pattern

### 4.1 Interface (Domain Layer)

```csharp
// ILeaveRequestRepository.cs
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

### 4.2 Implementation (Infrastructure Layer)

```csharp
// LeaveRequestRepository.cs
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
            .Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        await _context.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
        // ไม่ต้อง SaveChanges ที่นี่ — ให้ Service เป็นคนเรียก
    }
}
```

**กฎ:**
- Repository รับผิดชอบเฉพาะ data access ไม่มี business logic
- ไม่เรียก `SaveChangesAsync()` ใน Repository — ให้ Service เรียก
- ใช้ `CancellationToken` ทุก async method
- ใช้ `Include()` เฉพาะที่ caller ต้องการจริง ๆ

---

## 5. Service Pattern

### 5.1 Interface (Domain Layer)

```csharp
// ILeaveService.cs
public interface ILeaveService
{
    Task<LeaveRequestResponse> SubmitLeaveRequestAsync(CreateLeaveRequest request, string submittedBy, CancellationToken cancellationToken = default);
    Task<LeaveRequestResponse> ApproveLeaveAsync(int leaveRequestId, ApproveLeaveRequest request, string approvedBy, CancellationToken cancellationToken = default);
    Task<LeaveRequestResponse> RejectLeaveAsync(int leaveRequestId, RejectLeaveRequest request, string rejectedBy, CancellationToken cancellationToken = default);
    Task CancelLeaveAsync(int leaveRequestId, string cancelledBy, CancellationToken cancellationToken = default);
}
```

### 5.2 Implementation (Application Layer)

```csharp
// LeaveService.cs
public class LeaveService : ILeaveService
{
    private readonly ILeaveRequestRepository _leaveRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(
        ILeaveRequestRepository leaveRepository,
        ILeaveBalanceRepository balanceRepository,
        AppDbContext context,
        ILogger<LeaveService> logger)
    {
        _leaveRepository = leaveRepository;
        _balanceRepository = balanceRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<LeaveRequestResponse> SubmitLeaveRequestAsync(
        CreateLeaveRequest request,
        string submittedBy,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate
        var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(
            request.EmployeeId, request.LeaveType, cancellationToken);

        if (balance == null || balance.RemainingDays < request.TotalDays)
            throw new BusinessException("วันลาคงเหลือไม่เพียงพอ", "VR-002");

        // 2. Create entity
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = request.TotalDays,
            Reason = request.Reason,
            Status = LeaveStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = submittedBy
        };

        // 3. Persist (transaction)
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _leaveRepository.AddAsync(leaveRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Leave request {Id} submitted by {User}", leaveRequest.Id, submittedBy);
            return MapToResponse(leaveRequest);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to submit leave request for employee {EmployeeId}", request.EmployeeId);
            throw;
        }
    }

    private static LeaveRequestResponse MapToResponse(LeaveRequest entity) => new()
    {
        Id = entity.Id,
        EmployeeId = entity.EmployeeId,
        LeaveType = entity.LeaveType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        TotalDays = entity.TotalDays,
        Status = entity.Status
    };
}
```

**กฎ Service:**
- Service เป็นเจ้าของ transaction — เรียก `SaveChangesAsync()` ที่นี่
- ใช้ `await using var transaction` สำหรับ multi-step operation
- ต้องมี try/catch ครอบ transaction block + log error
- mapping จาก entity → response ทำใน Service (ใช้ private static method หรือ extension)
- Business exception ใช้ custom exception class พร้อม error code

---

## 6. Controller Pattern

```csharp
// LeaveRequestsController.cs
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

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var submittedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await _leaveService.SubmitLeaveRequestAsync(request, submittedBy, cancellationToken);
        return CreatedAtAction(nameof(GetLeaveRequest), new { id = result.Id },
            ApiResponse<LeaveRequestResponse>.Success(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveRequest(int id, CancellationToken cancellationToken)
    {
        // implementation
    }
}
```

**กฎ Controller:**
- Controller ไม่มี business logic — เพียง delegate ไปยัง Service
- ใช้ `[ApiController]` และ `[Route("api/v1/[controller]")]`
- ใช้ `[Authorize]` ทุก controller ยกเว้น endpoint ที่ public จริง ๆ
- ระบุ `[ProducesResponseType]` ทุก action
- คืน `ApiResponse<T>` ทุก response (ดูส่วน 7)
- ดึง user จาก `User.FindFirst(ClaimTypes.NameIdentifier)` เท่านั้น

---

## 7. API Response Standard

```csharp
// ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public PaginationMeta? Metadata { get; set; }

    public static ApiResponse<T> Success(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string code, string message, IEnumerable<string>? details = null)
        => new()
        {
            Success = false,
            Error = new ApiError { Code = code, Message = message, Details = details?.ToList() }
        };
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
}
```

**HTTP Status Code:**

| สถานการณ์ | Status |
|----------|--------|
| สร้างสำเร็จ | 201 Created |
| อ่านสำเร็จ | 200 OK |
| แก้ไขสำเร็จ | 200 OK |
| ลบสำเร็จ | 204 No Content |
| ข้อมูลไม่ถูกต้อง (validation) | 400 Bad Request |
| ไม่ได้ login | 401 Unauthorized |
| ไม่มีสิทธิ์ | 403 Forbidden |
| ไม่พบข้อมูล | 404 Not Found |
| Business rule violation | 422 Unprocessable Entity |
| ข้อผิดพลาดระบบ | 500 Internal Server Error |

---

## 8. Error Handling

### 8.1 Custom Exception

```csharp
// BusinessException.cs
public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string message, string code = "BUSINESS_ERROR")
        : base(message)
    {
        Code = code;
    }
}

// NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.") { }
}
```

### 8.2 Global Exception Middleware

```csharp
// GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponse(context, 404, "NOT_FOUND", ex.Message);
        }
        catch (BusinessException ex)
        {
            await WriteErrorResponse(context, 422, ex.Code, ex.Message);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors.Select(e => e.ErrorMessage);
            await WriteErrorResponse(context, 400, "VALIDATION_ERROR", "ข้อมูลไม่ถูกต้อง", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, 500, "INTERNAL_ERROR", "เกิดข้อผิดพลาดภายในระบบ");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context, int statusCode, string code, string message,
        IEnumerable<string>? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail(code, message, details);
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

**กฎ Error Handling:**
- ห้าม try/catch ใน Controller (ให้ middleware จัดการ)
- throw `BusinessException` เมื่อ business rule ถูก violate
- throw `NotFoundException` เมื่อหาข้อมูลไม่พบ
- throw `ValidationException` เมื่อ input ไม่ผ่าน validation (FluentValidation)
- ห้าม return error จาก Repository — ให้ throw exception
- ห้าม expose stack trace หรือ internal detail ให้ client

---

## 9. Validation (FluentValidation)

```csharp
// CreateLeaveRequestValidator.cs
public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId ไม่ถูกต้อง");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("วันที่เริ่มลาต้องไม่เป็นอดีต");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("วันสิ้นสุดต้องไม่ก่อนวันเริ่มลา");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("กรุณาระบุเหตุผลการลา")
            .MaximumLength(500).WithMessage("เหตุผลต้องไม่เกิน 500 ตัวอักษร");
    }
}
```

**กฎ Validation:**
- ใช้ FluentValidation เป็นมาตรฐาน — ห้าม Data Annotation สำหรับ business validation
- Register validator ผ่าน DI: `services.AddValidatorsFromAssembly()`
- Validation ทำงานอัตโนมัติผ่าน `[ApiController]` attribute

---

## 10. Logging

```csharp
// ใช้ structured logging เสมอ
_logger.LogInformation("Leave request {Id} submitted by {User}", leaveRequest.Id, submittedBy);
_logger.LogError(ex, "Failed to submit leave request for employee {EmployeeId}", request.EmployeeId);
_logger.LogWarning("Leave balance insufficient for employee {EmployeeId}, requested {Days} days", employeeId, requestedDays);
```

**กฎ Logging:**
- ใช้ structured logging (message template + arguments) ไม่ใช่ string interpolation
- `LogInformation`: operation สำเร็จ, business milestone
- `LogWarning`: situation ที่ผิดปกติแต่ระบบยังทำงานได้
- `LogError`: exception ที่ไม่คาดคิด, data corruption, external system failure
- ห้าม log ข้อมูล sensitive (password, token, PII)

---

## 11. Dependency Injection Registration

```csharp
// Program.cs
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**กฎ DI:**
- ใช้ `AddScoped` สำหรับ Repository และ Service (tied to HTTP request lifecycle)
- ใช้ `AddSingleton` เฉพาะ stateless service เช่น configuration, cache
- ห้าม `new` service ใน class — ต้องผ่าน constructor injection เสมอ

---

## 12. OOP Best Practices สรุป

| หลักการ | การนำไปใช้ |
|---------|----------|
| Single Responsibility | แต่ละ class มีความรับผิดชอบเดียว (Entity เก็บ data, Service มี logic, Repository เข้าถึง DB) |
| Open/Closed | เพิ่ม feature ผ่าน interface ใหม่/implementation ใหม่ ไม่แก้ class เดิม |
| Liskov Substitution | Implementation ต้องใช้แทน Interface ได้โดยไม่เปลี่ยน behavior |
| Interface Segregation | แยก Interface ตามกลุ่มการใช้งาน ไม่รวมทุก method ไว้ใน interface เดียว |
| Dependency Inversion | depend กับ interface (abstraction) ไม่ depend กับ concrete class |
| Async/Await | ใช้ `async Task` ทุก method ที่ access I/O (DB, HTTP, File) |
| Immutability | DTO เป็น record หรือ init-only properties เมื่อทำได้ |
| Null safety | ใช้ `?` nullable annotation ให้ครบ, ใช้ `??` และ `?.` แทน null check ยาว |

---

## 13. SQLite-Specific Constraints

| ข้อจำกัด | การจัดการ |
|---------|---------|
| ไม่มี `NVARCHAR` | ใช้ `TEXT` → EF Core map เป็น `string` |
| `DECIMAL` ไม่ precise | ใช้ `REAL` → ระบุ `HasColumnType("REAL")` ใน config |
| ไม่รองรับ `GETDATE()` | ใช้ `DateTime.UtcNow` ใน C# ก่อน save |
| Auto-increment PK | ใช้ `int Id` → EF Core จัดการ autoincrement อัตโนมัติ |
| ไม่มี `UNIQUEIDENTIFIER` | ใช้ `Guid` หรือ `int` แล้วแต่ design |
| Foreign key ต้อง enable | เพิ่ม `PRAGMA foreign_keys = ON;` ใน `OnConfiguring` |

```csharp
// AppDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlite(connectionString,
        x => x.MigrationsAssembly("LeaveRequest.Infrastructure"));
}

// เปิด FK enforcement ใน SQLite
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply all configurations from assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```
