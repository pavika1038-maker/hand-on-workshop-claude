# Unit Test Standard — Leave Request and Approval System

> มาตรฐาน Unit Test สำหรับ Workshop นี้  
> Stack: xUnit + Moq + FluentAssertions (.NET 10)  
> อัปเดตล่าสุด: 2026-06-16

---

## 1. หลักการทั่วไป

- Unit Test ต้องทดสอบ **Application Layer** เป็นหลัก (Services)
- ไม่ทดสอบ Infrastructure (Repository) โดยตรง — ใช้ Mock แทน
- ทุก test ต้องรันได้อิสระจากกัน (no shared state)
- ห้าม test แบบ order-dependent

---

## 2. โครงสร้าง Project

```
tests/
└── LeaveRequest.Application.Tests/
    ├── Services/
    │   ├── LeaveServiceTests.cs
    │   ├── ApprovalServiceTests.cs
    │   └── ReportServiceTests.cs
    └── LeaveRequest.Application.Tests.csproj
```

### 2.1 csproj dependencies

```xml
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
```

---

## 3. Naming Convention

### 3.1 Test Class

```
{ClassName}Tests
```

ตัวอย่าง: `LeaveServiceTests`, `ApprovalServiceTests`

### 3.2 Test Method

```
{MethodName}_Should_{ExpectedBehavior}_When_{Condition}
```

| ส่วน | ความหมาย | ตัวอย่าง |
|------|----------|---------|
| `MethodName` | ชื่อ method ที่ test | `SubmitLeaveRequest` |
| `Should` | คำเชื่อมคงที่ | `Should` |
| `ExpectedBehavior` | ผลลัพธ์ที่คาดหวัง | `ReturnSuccess`, `ThrowException`, `CreatePendingRequest` |
| `When` | คำเชื่อมคงที่ | `When` |
| `Condition` | เงื่อนไขที่ทำให้เกิดผลนั้น | `ValidRequest`, `InsufficientBalance`, `EmployeeNotFound` |

ตัวอย่างชื่อ test ที่ถูกต้อง:
```csharp
SubmitLeaveRequest_Should_CreatePendingRequest_When_ValidRequest
SubmitLeaveRequest_Should_ReturnError_When_InsufficientBalance
SubmitLeaveRequest_Should_ReturnError_When_EmployeeNotFound
SubmitLeaveRequest_Should_ReturnError_When_StartDateInPast
ApproveLeaveRequest_Should_UpdateStatusToApproved_When_ManagerApproves
ApproveLeaveRequest_Should_ReturnError_When_RequesterIsNotSubordinate
```

---

## 4. AAA Pattern

ทุก test method ต้องมีโครงสร้าง 3 ส่วน และแยกด้วย blank line + comment:

```csharp
[Fact]
public async Task SubmitLeaveRequest_Should_CreatePendingRequest_When_ValidRequest()
{
    // Arrange
    var employeeId = "EMP001";
    var request = new SubmitLeaveRequestCommand { ... };
    _mockLeaveRepo.Setup(...).ReturnsAsync(...);

    // Act
    var result = await _sut.SubmitLeaveRequestAsync(employeeId, request);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data!.Status.Should().Be("Pending");
}
```

---

## 5. Mock / Stub Guideline

### 5.1 ใช้ Mock เมื่อ

- Repository interface (ILeaveRequestRepository, ILeaveBalanceRepository, IEmployeeRepository)
- Logger (ILogger<T>)
- External service stub (IEmailService, ISlaService)

### 5.2 ใช้ SQLite In-Memory เมื่อ

- ต้องการ transaction จริง (SaveChangesAsync ที่ทำงานได้)
- ทดสอบ EF Core query logic
- **อย่าใช้ EF Core InMemory** — ไม่รองรับ transaction และ unique constraint

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=:memory:")
    .Options;
_dbContext = new AppDbContext(options);
_dbContext.Database.EnsureCreated();
```

### 5.3 Setup Pattern

```csharp
// Return value
_mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

// Return null (not found)
_mockRepo.Setup(r => r.GetByIdAsync("NOTEXIST")).ReturnsAsync((Entity?)null);

// Capture argument (สำหรับ verify ค่าที่ถูก save)
Entities.LeaveRequest? captured = null;
_mockLeaveRepo
    .Setup(r => r.AddAsync(It.IsAny<Entities.LeaveRequest>()))
    .Callback<Entities.LeaveRequest>(r => captured = r);
```

### 5.4 Verify ว่า Method ถูกเรียก

```csharp
_mockLeaveRepo.Verify(r => r.AddAsync(It.IsAny<Entities.LeaveRequest>()), Times.Once);
_mockEmailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
```

---

## 6. FluentAssertions Guideline

```csharp
// Boolean
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();

// Null check
result.Data.Should().NotBeNull();
result.Data.Should().BeNull();

// Value equality
result.Data!.Status.Should().Be("Pending");
result.ErrorCode.Should().Be("INSUFFICIENT_BALANCE");

// Collection
list.Should().HaveCount(3);
list.Should().Contain(x => x.EmployeeId == "EMP001");
list.Should().BeEmpty();

// Exception
var act = async () => await _sut.SubmitLeaveRequestAsync(null!, command);
await act.Should().ThrowAsync<ArgumentNullException>();
```

---

## 7. Test Class Setup / Teardown

```csharp
public class LeaveServiceTests : IDisposable
{
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRepo;
    private readonly LeaveService _sut; // System Under Test
    private readonly AppDbContext _dbContext;

    public LeaveServiceTests()
    {
        // Initialize mocks และ SUT ใน constructor
        _mockLeaveRepo = new Mock<ILeaveRequestRepository>();
        // ...
        _sut = new LeaveService(...);
    }

    public void Dispose()
    {
        _dbContext.Dispose(); // cleanup SQLite in-memory
    }
}
```

---

## 8. สิ่งที่ต้อง Test (Priority)

| Priority | เนื้อหา | SCR |
|----------|---------|-----|
| P1 — Must | Input validation (null, empty, length, range) | SCR-003 |
| P1 — Must | Business rule: insufficient leave balance | SCR-003 |
| P1 — Must | Business rule: start date ≥ today | SCR-003 |
| P1 — Must | Happy path: submit → Pending status | SCR-003 |
| P1 — Must | Happy path: approve → Approved + balance deducted | SCR-004 |
| P2 — Should | Reject → Rejected + balance restored | SCR-004 |
| P2 — Should | Cancel (Pending) → Cancelled immediately | SCR-006 |
| P2 — Should | Cancel (Approved) → CancelRequested | SCR-006 |
| P3 — Nice | Report filter logic | SCR-010 |

---

## 9. สิ่งที่ไม่ต้อง Test ใน Unit Test

- Controller routing/middleware (ใช้ Integration Test แทน)
- Database migration
- Frontend React components
- EF Core query translation (ใช้ Integration Test แทน)
