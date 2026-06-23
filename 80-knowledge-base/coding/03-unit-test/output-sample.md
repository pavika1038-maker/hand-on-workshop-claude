# Unit Test Sample — LeaveService.SubmitLeaveRequest

> ตัวอย่าง unit test สำหรับ `LeaveService.SubmitLeaveRequestAsync()`  
> ครอบคลุม: SFR-001, VR-001, VR-002, VR-003

---

```csharp
// File: tests/LeaveRequest.Application.Tests/Services/LeaveServiceTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LeaveRequest.Application.Tests.Services;

public class LeaveServiceTests : IDisposable
{
    // Mocks
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRepo;
    private readonly Mock<ILeaveBalanceRepository> _mockBalanceRepo;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
    private readonly Mock<ILogger<LeaveService>> _mockLogger;
    private readonly AppDbContext _dbContext;

    // SUT
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        _mockLeaveRepo = new Mock<ILeaveRequestRepository>();
        _mockBalanceRepo = new Mock<ILeaveBalanceRepository>();
        _mockEmployeeRepo = new Mock<IEmployeeRepository>();
        _mockLogger = new Mock<ILogger<LeaveService>>();

        // SQLite in-memory database (รองรับ transaction จริง ต่างจาก EF InMemory)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new LeaveService(
            _mockLeaveRepo.Object,
            _mockBalanceRepo.Object,
            _mockEmployeeRepo.Object,
            _dbContext,
            _mockLogger.Object
        );
    }

    // ─────────────────────────────────────────────────────────────────
    // Happy Path
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequest_Should_CreatePendingRequest_When_ValidRequest()
    {
        // Arrange
        var request = new CreateLeaveRequest(
            EmployeeId: 1,
            LeaveTypeId: 1,
            StartDate: DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            EndDate: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            TotalDays: 3,
            Reason: "พักผ่อนประจำปี"
        );

        var mockBalance = new LeaveBalance
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            Year = DateTime.UtcNow.Year,
            TotalDays = 10,
            UsedDays = 2,
            RemainingDays = 8    // มากกว่า 3 — ผ่าน VR-002
        };

        _mockBalanceRepo
            .Setup(x => x.GetByEmployeeAndTypeAsync(1, 1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockBalance);

        _mockLeaveRepo
            .Setup(x => x.GetOverlappingAsync(1,
                request.StartDate, request.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.LeaveRequest>());   // ไม่มี overlap — ผ่าน VR-003

        var savedEntity = new Domain.Entities.LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = 3,
            Reason = request.Reason,
            Status = LeaveStatus.PendingApproval,
            Employee = new Employee { Id = 1, FirstName = "สมชาย", LastName = "ใจดี" },
            LeaveType = new Domain.Entities.LeaveType { Id = 1, Name = "ลาพักร้อน" },
            CreatedAt = DateTime.UtcNow
        };

        _mockLeaveRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync(request, "test-user");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("PendingApproval");
        result.TotalDays.Should().Be(3);
        result.EmployeeName.Should().Be("สมชาย ใจดี");

        _mockLeaveRepo.Verify(
            x => x.AddAsync(It.Is<Domain.Entities.LeaveRequest>(r =>
                r.EmployeeId == 1 &&
                r.Status == LeaveStatus.PendingApproval &&
                r.CreatedBy == "test-user"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────
    // VR-001: วันเริ่มลาต้องไม่เป็นอดีต (ตรวจโดย FluentValidation)
    // (test ที่ controller/validator level — service ไม่รับ request นี้)
    // ─────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────
    // VR-002: วันลาคงเหลือไม่เพียงพอ
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequest_Should_ThrowBusinessException_When_InsufficientBalance()
    {
        // Arrange
        var request = new CreateLeaveRequest(
            EmployeeId: 1,
            LeaveTypeId: 1,
            StartDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EndDate: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            TotalDays: 5,
            Reason: "ลาพักร้อน"
        );

        var mockBalance = new LeaveBalance
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            Year = DateTime.UtcNow.Year,
            TotalDays = 10,
            UsedDays = 8,
            RemainingDays = 2    // น้อยกว่า 5 — fail VR-002
        };

        _mockBalanceRepo
            .Setup(x => x.GetByEmployeeAndTypeAsync(1, 1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockBalance);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync(request, "test-user");

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("*วันลาคงเหลือไม่เพียงพอ*");

        _mockLeaveRepo.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.LeaveRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);    // ต้องไม่มีการบันทึก
    }

    [Fact]
    public async Task SubmitLeaveRequest_Should_ThrowBusinessException_When_BalanceNotFound()
    {
        // Arrange
        var request = new CreateLeaveRequest(
            EmployeeId: 999,
            LeaveTypeId: 1,
            StartDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EndDate: DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            TotalDays: 3,
            Reason: "ลาพักร้อน"
        );

        _mockBalanceRepo
            .Setup(x => x.GetByEmployeeAndTypeAsync(999, 1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveBalance?)null);    // ไม่พบข้อมูล balance

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync(request, "test-user");

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("*ไม่พบข้อมูลวันลา*");
    }

    // ─────────────────────────────────────────────────────────────────
    // VR-003: ไม่มีคำร้องซ้ำในช่วงวันเดียวกัน
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequest_Should_ThrowBusinessException_When_OverlappingRequestExists()
    {
        // Arrange
        var request = new CreateLeaveRequest(
            EmployeeId: 1,
            LeaveTypeId: 1,
            StartDate: DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            EndDate: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            TotalDays: 3,
            Reason: "ลาพักร้อน"
        );

        _mockBalanceRepo
            .Setup(x => x.GetByEmployeeAndTypeAsync(1, 1, DateTime.UtcNow.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaveBalance { RemainingDays = 10 });

        var existingRequest = new Domain.Entities.LeaveRequest
        {
            Id = 5,
            EmployeeId = 1,
            Status = LeaveStatus.PendingApproval,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(4)),   // overlap!
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(6))
        };

        _mockLeaveRepo
            .Setup(x => x.GetOverlappingAsync(1,
                request.StartDate, request.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.LeaveRequest> { existingRequest });

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync(request, "test-user");

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("*คำร้องขอลาในช่วงวันเดียวกัน*");
    }

    // ─────────────────────────────────────────────────────────────────
    // Cancel Leave Request
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelLeaveRequest_Should_SetStatusCancelled_When_RequestIsPending()
    {
        // Arrange
        var pendingRequest = new Domain.Entities.LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            Status = LeaveStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "employee1"
        };

        _mockLeaveRepo
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingRequest);

        // Act
        await _sut.CancelLeaveRequestAsync(1, "employee1");

        // Assert
        _mockLeaveRepo.Verify(
            x => x.UpdateAsync(It.Is<Domain.Entities.LeaveRequest>(r =>
                r.Status == LeaveStatus.Cancelled &&
                r.UpdatedBy == "employee1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(LeaveStatus.Approved)]
    [InlineData(LeaveStatus.Rejected)]
    public async Task CancelLeaveRequest_Should_ThrowBusinessException_When_StatusIsTerminal(
        LeaveStatus terminalStatus)
    {
        // Arrange
        var request = new Domain.Entities.LeaveRequest
        {
            Id = 1,
            Status = terminalStatus
        };

        _mockLeaveRepo
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var act = async () => await _sut.CancelLeaveRequestAsync(1, "employee1");

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("*ไม่สามารถยกเลิก*");
    }

    [Fact]
    public async Task CancelLeaveRequest_Should_ThrowNotFoundException_When_RequestDoesNotExist()
    {
        // Arrange
        _mockLeaveRepo
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.LeaveRequest?)null);

        // Act
        var act = async () => await _sut.CancelLeaveRequestAsync(99, "employee1");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─────────────────────────────────────────────────────────────────
    // Cleanup
    // ─────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
```

---

## Run Command

```bash
# Run tests ทั้งหมด
dotnet test

# Run tests พร้อม coverage
dotnet test --collect:"XPlat Code Coverage"

# Run เฉพาะ class นี้
dotnet test --filter "FullyQualifiedName~LeaveServiceTests"
```
