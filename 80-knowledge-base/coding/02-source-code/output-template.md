# Source Code Output Template

> Template สำหรับ source code ที่ generate รายฟังก์ชัน  
> ใช้ร่วมกับ `00-governance/knowledge.md` (coding standard) และ `01-sql-ddl/output-template.md`  
> Tech Stack: .NET 8, ASP.NET Core Web API, EF Core, SQLite, C# OOP

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Use Case | {use_case_name} |
| Function ID | {function_id} เช่น SCR-001, COM-001 |
| Method Signature Source | {method_signature_file} |
| Sequence Diagram Source | {sequence_diagram_file} |
| Generated Date | {date} |

---

## 2. Layer Structure ที่ต้อง Generate

สำหรับแต่ละ use case ให้ generate ตามลำดับ:

```
1. Domain Layer  → Entity (ถ้ายังไม่มี), Interface (Repository + Service)
2. Application Layer → DTO, Validator, Service Implementation
3. Infrastructure Layer → Repository Implementation, EF Core Config (ถ้าใหม่)
4. API Layer → Controller Action
```

---

## 3. Template: Entity

```csharp
// File: src/LeaveRequest.Domain/Entities/{EntityName}.cs
namespace LeaveRequest.Domain.Entities;

public class {EntityName}
{
    public int Id { get; set; }

    // --- {กลุ่ม property แรก} ---
    public {Type} {PropertyName} { get; set; } = {default_value};

    // --- Audit Columns ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // --- Navigation Properties ---
    public {RelatedEntity} {NavigationProperty} { get; set; } = null!;
    public ICollection<{RelatedEntity}> {CollectionProperty} { get; set; } = new List<{RelatedEntity}>();
}
```

---

## 4. Template: Repository Interface

```csharp
// File: src/LeaveRequest.Domain/Interfaces/Repositories/I{Entity}Repository.cs
namespace LeaveRequest.Domain.Interfaces.Repositories;

public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<{Entity}>> GetBy{Field}Async({Type} {field}, CancellationToken cancellationToken = default);
    Task AddAsync({Entity} entity, CancellationToken cancellationToken = default);
    Task UpdateAsync({Entity} entity, CancellationToken cancellationToken = default);
    Task DeleteAsync({Entity} entity, CancellationToken cancellationToken = default);
}
```

---

## 5. Template: Service Interface

```csharp
// File: src/LeaveRequest.Domain/Interfaces/Services/I{Domain}Service.cs
namespace LeaveRequest.Domain.Interfaces.Services;

public interface I{Domain}Service
{
    Task<{ResponseDTO}> {ActionName}Async({RequestDTO} request, string performedBy, CancellationToken cancellationToken = default);
    Task<{ResponseDTO}?> Get{Entity}ByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<{ResponseDTO}>> GetAll{Entities}Async(CancellationToken cancellationToken = default);
}
```

---

## 6. Template: DTO

```csharp
// File: src/LeaveRequest.Application/DTOs/{EntityName}DTOs.cs
namespace LeaveRequest.Application.DTOs;

// Request DTO — input จาก client
public record Create{Entity}Request(
    int {RequiredField},
    {Type} {RequiredField2},
    string? {OptionalField} = null
);

// Response DTO — output ไปยัง client
public record {Entity}Response(
    int Id,
    {Type} {Field1},
    {Type} {Field2},
    DateTime CreatedAt
);
```

---

## 7. Template: Validator

```csharp
// File: src/LeaveRequest.Application/Validators/Create{Entity}RequestValidator.cs
using FluentValidation;

namespace LeaveRequest.Application.Validators;

public class Create{Entity}RequestValidator : AbstractValidator<Create{Entity}Request>
{
    public Create{Entity}RequestValidator()
    {
        RuleFor(x => x.{RequiredField})
            .{ValidationRule}()
            .WithMessage("{error_message}");

        RuleFor(x => x.{DateField})
            .{ValidationRule}()
            .WithMessage("{error_message}");
    }
}
```

---

## 8. Template: Repository Implementation

```csharp
// File: src/LeaveRequest.Infrastructure/Repositories/{Entity}Repository.cs
using Microsoft.EntityFrameworkCore;

namespace LeaveRequest.Infrastructure.Repositories;

public class {Entity}Repository : I{Entity}Repository
{
    private readonly AppDbContext _context;

    public {Entity}Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<{Entity}?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.{Entities}
            .Include(x => x.{NavigationProperty})  // include เฉพาะที่จำเป็น
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync({Entity} entity, CancellationToken cancellationToken = default)
    {
        await _context.{Entities}.AddAsync(entity, cancellationToken);
        // ไม่ SaveChanges ที่นี่ — Service เป็นคนเรียก
    }

    public async Task UpdateAsync({Entity} entity, CancellationToken cancellationToken = default)
    {
        _context.{Entities}.Update(entity);
        // ไม่ SaveChanges ที่นี่
    }

    public async Task DeleteAsync({Entity} entity, CancellationToken cancellationToken = default)
    {
        _context.{Entities}.Remove(entity);
        // ไม่ SaveChanges ที่นี่
    }
}
```

---

## 9. Template: Service Implementation

```csharp
// File: src/LeaveRequest.Application/Services/{Domain}Service.cs
namespace LeaveRequest.Application.Services;

public class {Domain}Service : I{Domain}Service
{
    private readonly I{Entity}Repository _{entityRepository};
    private readonly AppDbContext _context;
    private readonly ILogger<{Domain}Service> _logger;

    public {Domain}Service(
        I{Entity}Repository {entityRepository},
        AppDbContext context,
        ILogger<{Domain}Service> logger)
    {
        _{entityRepository} = {entityRepository};
        _context = context;
        _logger = logger;
    }

    public async Task<{ResponseDTO}> {ActionName}Async(
        {RequestDTO} request,
        string performedBy,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Validate business rules
        // (validation ผ่าน FluentValidation จะถูก throw ก่อนถึง method นี้)
        // ตรวจ business rule ที่ต้องอ่าน DB เพิ่มเติม

        // Step 2: Build entity
        var entity = new {Entity}
        {
            {Property} = request.{Property},
            CreatedAt = DateTime.UtcNow,
            CreatedBy = performedBy
        };

        // Step 3: Persist (ใช้ transaction ถ้ามี multiple operations)
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _{entityRepository}.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("{Entity} {Id} created by {User}", nameof({Entity}), entity.Id, performedBy);
            return MapToResponse(entity);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create {Entity} for {User}", nameof({Entity}), performedBy);
            throw;
        }
    }

    private static {ResponseDTO} MapToResponse({Entity} entity) => new(
        entity.Id,
        entity.{Field1},
        entity.{Field2},
        entity.CreatedAt
    );
}
```

---

## 10. Template: Controller Action

```csharp
// File: src/LeaveRequest.API/Controllers/{Entities}Controller.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class {Entities}Controller : ControllerBase
{
    private readonly I{Domain}Service _{domainService};
    private readonly ILogger<{Entities}Controller> _logger;

    public {Entities}Controller(I{Domain}Service {domainService}, ILogger<{Entities}Controller> logger)
    {
        _{domainService} = {domainService};
        _logger = logger;
    }

    /// <summary>
    /// {อธิบาย action}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<{ResponseDTO}>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create{Entity}(
        [FromBody] Create{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var performedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await _{domainService}.{ActionName}Async(request, performedBy, cancellationToken);
        return CreatedAtAction(nameof(Get{Entity}), new { id = result.Id },
            ApiResponse<{ResponseDTO}>.Success(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<{ResponseDTO}>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get{Entity}(int id, CancellationToken cancellationToken)
    {
        var result = await _{domainService}.Get{Entity}ByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof({Entity}), id);
        return Ok(ApiResponse<{ResponseDTO}>.Success(result));
    }
}
```

---

## 11. Checklist ก่อน Submit Code

- [ ] Entity มี audit columns ครบ (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- [ ] Repository ไม่เรียก `SaveChangesAsync()` — Service เรียก
- [ ] Service ใช้ `await using var transaction` เมื่อมี multi-step write
- [ ] Exception handling: throw `BusinessException` / `NotFoundException` ที่เหมาะสม
- [ ] Controller ไม่มี business logic
- [ ] Controller คืน `ApiResponse<T>` ทุก action
- [ ] ทุก async method มี `CancellationToken` parameter
- [ ] Logging ใช้ structured template (ไม่ใช่ string interpolation)
- [ ] Validator ครอบคลุม required fields และ business constraint
- [ ] ชื่อ class, method, property ตรงตาม naming convention

---

## 12. Deviation Log Template

| # | Item | Expected (Design) | Actual (Code) | Reason | Status |
|---|------|------------------|---------------|--------|--------|
| 1 | {description} | {design_spec} | {code_actual} | {reason} | Open / Accepted |
