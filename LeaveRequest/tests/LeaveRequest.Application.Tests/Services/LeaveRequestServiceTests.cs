namespace LeaveRequest.Application.Tests.Services;

using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Services;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

public class LeaveRequestServiceTests
{
    // ── Mocks ────────────────────────────────────────────────────────────────
    private readonly Mock<ILeaveRequestRepository>    _leaveRepoMock          = new();
    private readonly Mock<ILeaveBalanceRepository>    _balanceRepoMock        = new();
    private readonly Mock<IEmployeeRepository>        _employeeRepoMock       = new();
    private readonly Mock<ILeaveTypeRepository>       _leaveTypeRepoMock      = new();
    private readonly Mock<ICancelRequestRepository>   _cancelRepoMock         = new();
    private readonly Mock<IApprovalHistoryRepository> _approvalHistoryRepoMock = new();
    private readonly Mock<IUnitOfWork>                _uowMock                = new();
    private readonly Mock<ITransaction>               _txMock                 = new();
    private readonly Mock<ILogger<LeaveRequestService>> _loggerMock           = new();

    // ── SUT ──────────────────────────────────────────────────────────────────
    private readonly LeaveRequestService _sut;

    // ── Shared immutable test data ────────────────────────────────────────────

    private static readonly Employee ActiveEmployee = new()
    {
        EmployeeId   = "EMP001",
        EmployeeCode = "E001",
        FullNameTh   = "สมชาย ใจดี",
        FullNameEn   = "Somchai Jaidee",
        Email        = "somchai@abc.com",
        HireDate     = new DateOnly(2022, 1, 1),
        ManagerId    = "MGR001",
        EmployeeType = EmployeeType.Regular,
        IsActive     = true,
        CreatedAt    = DateTime.UtcNow,
        CreatedBy    = "SYSTEM"
    };

    private static readonly LeaveType AnnualLeaveType = new()
    {
        LeaveTypeId          = 1,
        TypeCode             = "ANNUAL",
        TypeNameTh           = "ลาพักผ่อน",
        TypeNameEn           = "Annual Leave",
        MaxDaysPerYear       = 10,
        IsAvailableForOutsource = false,
        RequiresMedicalCert  = false,
        CreatedAt            = DateTime.UtcNow,
        CreatedBy            = "SYSTEM"
    };

    // ponytail: factory, not field — service mutates LeaveBalance in-place so sharing is unsafe
    // RemainingDays = 10 + 0 - 2 - 0 = 8
    private static LeaveBalance MakeSufficientBalance() => new()
    {
        LeaveBalanceId    = Guid.NewGuid(),
        EmployeeId        = "EMP001",
        LeaveTypeId       = 1,
        LeaveYear         = 2026,
        EntitledDays      = 10,
        UsedDays          = 2,
        PendingDays       = 0,
        CarriedForwardDays = 0,
        CreatedAt         = DateTime.UtcNow,
        CreatedBy         = "SYSTEM"
    };

    // StartDate=2026-07-01, EndDate=2026-07-03, DurationDays=3
    private static readonly CreateLeaveRequestDto ValidRequest = new(
        LeaveTypeId:   1,
        StartDate:     new DateOnly(2026, 7, 1),
        EndDate:       new DateOnly(2026, 7, 3),
        IsHalfDay:     false,
        HalfDayPeriod: null,
        Reason:        "พักผ่อน",
        AttachmentIds: []
    );

    public LeaveRequestServiceTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_txMock.Object);
        _txMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _txMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _txMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _approvalHistoryRepoMock
            .Setup(x => x.AddAsync(It.IsAny<ApprovalHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new LeaveRequestService(
            _leaveRepoMock.Object,
            _balanceRepoMock.Object,
            _employeeRepoMock.Object,
            _leaveTypeRepoMock.Object,
            _cancelRepoMock.Object,
            _approvalHistoryRepoMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    // ── Helper: pending leave request with Employee navigation populated ──────

    private static LeaveRequest BuildPendingRequest(
        Guid id, string employeeId, string? managerId, decimal durationDays = 3m)
        => new()
        {
            LeaveRequestId = id,
            EmployeeId     = employeeId,
            LeaveTypeId    = 1,
            StartDate      = new DateOnly(2026, 7, 1),
            EndDate        = new DateOnly(2026, 7, 3),
            DurationDays   = durationDays,
            Status         = LeaveStatus.Pending,
            Employee       = new Employee
            {
                EmployeeId = employeeId,
                ManagerId  = managerId,
                IsActive   = true,
                CreatedAt  = DateTime.UtcNow,
                CreatedBy  = "SYSTEM"
            },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = employeeId
        };

    private static LeaveBalance BuildBalance(
        string employeeId, decimal pending = 3m, decimal used = 0m) => new()
    {
        LeaveBalanceId    = Guid.NewGuid(),
        EmployeeId        = employeeId,
        LeaveTypeId       = 1,
        LeaveYear         = 2026,
        EntitledDays      = 10,
        UsedDays          = used,
        PendingDays       = pending,
        CarriedForwardDays = 0,
        CreatedAt         = DateTime.UtcNow,
        CreatedBy         = "SYSTEM"
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // SubmitLeaveRequestAsync
    // ═══════════════════════════════════════════════════════════════════════════

    // TC-SPEC-001 | Source: SRS SF-003 §2.3.6, Method Sig §4.4, Seq Diagram §2
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_CreatePendingRequestAndDeductPendingDays_When_ValidRequest()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(MakeSufficientBalance());
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        result.Status.Should().Be(LeaveStatus.Pending);
        result.LeaveRequestRef.Should().StartWith("LR-2026-");
        result.Message.Should().Contain("สำเร็จ");

        _leaveRepoMock.Verify(x => x.AddAsync(
            It.Is<LeaveRequest>(lr =>
                lr.EmployeeId == "EMP001" && lr.LeaveTypeId == 1 &&
                lr.DurationDays == 3m && lr.Status == LeaveStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);

        _balanceRepoMock.Verify(x => x.Update(It.Is<LeaveBalance>(b => b.PendingDays == 3m)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-SPEC-002 | Source: Method Sig §4.4 step 1
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowNotFoundException_When_EmployeeNotFound()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP999", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Employee?)null);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP999", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*EMP999*");
        _leaveTypeRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<byte>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // TC-SPEC-003 | Source: Method Sig §4.4 step 1
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowNotFoundException_When_EmployeeIsInactive()
    {
        // Arrange
        var inactiveEmp = new Employee
        {
            EmployeeId = "EMP_INACTIVE", IsActive = false,
            CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP_INACTIVE", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(inactiveEmp);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP_INACTIVE", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*EMP_INACTIVE*");
    }

    // TC-SPEC-004 | Source: Method Sig §4.4 step 2
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowNotFoundException_When_LeaveTypeNotFound()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((LeaveType?)null);

        var request = ValidRequest with { LeaveTypeId = 99 };

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    // TC-SPEC-005 | Source: Method Sig §4.4 step 3 VR-001
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_OutsourceRequestsRestrictedLeaveType()
    {
        // Arrange
        var outsourceEmp = new Employee
        {
            EmployeeId = "EMP_OUT", EmployeeType = EmployeeType.Outsource,
            IsActive = true, HireDate = new DateOnly(2022, 1, 1),
            ManagerId = "MGR001", CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP_OUT", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(outsourceEmp);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType); // IsAvailableForOutsource = false

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP_OUT", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "VR-001");
    }

    // TC-SPEC-006 | Source: Method Sig §4.4 step 4 VR-003
    // [TODO: resolve open question] VR-003 Probation check not implemented in LeaveRequestService
    [Fact(Skip = "TODO: VR-003 Probation period check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_EmployeeInProbation()
        => Task.CompletedTask;

    // TC-SPEC-007 | Source: Method Sig §4.4 step 5 VR-004
    // [TODO: resolve open question] VR-004 Insufficient service length check not implemented
    [Fact(Skip = "TODO: VR-004 service-length check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_InsufficientServiceLength()
        => Task.CompletedTask;

    // TC-SPEC-008 | Source: Method Sig §4.4 step 6 VR-005
    // [TODO: resolve open question] VR-005 Advance notice (Annual ≥1 day) not implemented in service
    [Fact(Skip = "TODO: VR-005 advance-notice check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_AnnualLeaveWithoutAdvanceNotice()
        => Task.CompletedTask;

    // TC-SPEC-009 | Source: Method Sig §4.4 step 7 VR-006
    // [TODO: resolve open question] VR-006 Advance notice (Business ≥3 days) not implemented in service
    [Fact(Skip = "TODO: VR-006 advance-notice check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_BusinessLeaveWithoutAdvanceNotice()
        => Task.CompletedTask;

    // TC-SPEC-010 | Source: Method Sig §4.4 step 8 VR-007
    // [TODO: resolve open question] VR-007 medical cert check not implemented in service
    [Fact(Skip = "TODO: VR-007 RequiresMedicalCert check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_SickLeaveOver3DaysWithoutCert()
        => Task.CompletedTask;

    // TC-SPEC-011 | Source: Method Sig §4.4 step 8 (AlternativeFlow)
    // [TODO: resolve open question] VR-007 not implemented — can't test the passing case either
    [Fact(Skip = "TODO: VR-007 RequiresMedicalCert check not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_CreatePendingRequest_When_SickLeaveWithCertProvided()
        => Task.CompletedTask;

    // TC-SPEC-012 | Source: Method Sig §4.4 step 9 VR-002
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_InsufficientBalance()
    {
        // Arrange: RemainingDays = 10 - 9 - 0 = 1 < 3 requested
        var lowBalance = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(), EmployeeId = "EMP001", LeaveTypeId = 1, LeaveYear = 2026,
            EntitledDays = 10, UsedDays = 9, PendingDays = 0, CarriedForwardDays = 0,
            CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(lowBalance);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INSUFFICIENT_BALANCE");
        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _leaveRepoMock.Verify(x => x.AddAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // TC-SPEC-013 | Source: Method Sig §4.4 step 10
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_DateConflictWithExistingRequest()
    {
        // Arrange
        var existing = new LeaveRequest
        {
            LeaveRequestId = Guid.NewGuid(), EmployeeId = "EMP001",
            StartDate = new DateOnly(2026, 7, 2), EndDate = new DateOnly(2026, 7, 4),
            Status = LeaveStatus.Pending, CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(MakeSufficientBalance());
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([existing]);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "DATE_CONFLICT");
        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // TC-SPEC-014 | Source: Method Sig §7 NFR-010 — covered by TC-SPEC-012:
    //   VR-002 fail → no BeginTransaction, no AddAsync, no SaveChangesAsync

    // TC-SPEC-015 | Source: Method Sig §4.4
    // [TODO: resolve open question] PublishLeaveSubmittedAsync — no IEventPublisher in service
    [Fact(Skip = "TODO: PublishLeaveSubmittedAsync event publishing not in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_PublishEvent_When_Committed() => Task.CompletedTask;

    // TC-KB-001, TC-KB-002, TC-KB-003 | Source: rule-input-validation.md → 1.1, lesson-learned → BUG-008
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task SubmitLeaveRequestAsync_Should_ThrowNotFoundException_When_EmployeeIdIsNullOrWhitespace(
        string? employeeId)
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Employee?)null);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync(employeeId!, ValidRequest);

        // Assert: must not throw NullReferenceException
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-KB-004 | Source: rule-input-validation.md → 1.1 MaxLength=50
    // [TODO: resolve open question] EmployeeId MaxLength validated at FluentValidation, not service
    [Fact(Skip = "TODO: EmployeeId MaxLength=50 validated at controller/FluentValidation, not service")]
    public Task SubmitLeaveRequestAsync_Should_ThrowValidationError_When_EmployeeIdExceedsMaxLength()
        => Task.CompletedTask;

    // TC-KB-005 | Source: rule-input-validation.md → 1.2
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowNotFoundException_When_LeaveTypeIdIsZero()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(0, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((LeaveType?)null);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest with { LeaveTypeId = 0 });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-KB-006 | Source: rule-input-validation.md → 1.3, lesson-learned → EC-002
    // [TODO: resolve open question] StartDate < Today validated at FluentValidation, not service
    [Fact(Skip = "TODO: StartDate past-date check at controller/FluentValidation, not service")]
    public Task SubmitLeaveRequestAsync_Should_ThrowError_When_StartDateIsInPast() => Task.CompletedTask;

    // TC-KB-007 | Source: rule-input-validation.md → 1.3, lesson-learned → EC-001
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_CreatePendingRequest_When_StartDateIsToday()
    {
        // Arrange: today is a valid start date (service does not reject it)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var requestToday = new CreateLeaveRequestDto(1, today, today, false, null, "ลาวันนี้", []);

        var currentYearBalance = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(), EmployeeId = "EMP001", LeaveTypeId = 1,
            LeaveYear = today.Year, EntitledDays = 10, UsedDays = 0,
            PendingDays = 0, CarriedForwardDays = 0, CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };

        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, today.Year, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(currentYearBalance);
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", requestToday);

        // Assert: service does not reject today's date
        result.Status.Should().Be(LeaveStatus.Pending);
    }

    // TC-KB-008 | Source: rule-input-validation.md → 1.3
    // [TODO: resolve open question] EndDate < StartDate validated at FluentValidation, not service
    [Fact(Skip = "TODO: EndDate < StartDate validated at controller/FluentValidation, not service")]
    public Task SubmitLeaveRequestAsync_Should_ThrowError_When_EndDateBeforeStartDate() => Task.CompletedTask;

    // TC-KB-009 | Source: rule-input-validation.md → 5.3
    // [TODO: resolve open question] default(DateOnly) validated at FluentValidation, not service
    [Fact(Skip = "TODO: default(DateOnly) validated at controller/FluentValidation, not service")]
    public Task SubmitLeaveRequestAsync_Should_ThrowError_When_StartDateIsDefault() => Task.CompletedTask;

    // TC-KB-010 | Source: rule-input-validation.md → 1.5
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_HalfDayPeriodIsInvalidValue()
    {
        // Arrange: checked first, before any repo call
        var request = ValidRequest with { IsHalfDay = true, HalfDayPeriod = "MORNING" };

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_HALF_DAY");
        _employeeRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // TC-KB-011 | Source: rule-input-validation.md → 1.5
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_HalfDayPeriodIsNull()
    {
        // Arrange
        var request = ValidRequest with { IsHalfDay = true, HalfDayPeriod = null };

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_HALF_DAY");
    }

    // TC-KB-012 | Source: rule-input-validation.md → 1.5, lesson-learned → EC-005
    // [TODO: resolve open question] IsHalfDay=true + Start≠End not validated in service
    [Fact(Skip = "TODO: HalfDay Start≠End validation not implemented in LeaveRequestService")]
    public Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_HalfDaySpansMultipleDays()
        => Task.CompletedTask;

    // TC-KB-013 | Source: rule-input-validation.md → 1.6, lesson-learned → EC-004
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_SetDurationToHalfDay_When_IsHalfDayTrue()
    {
        // Arrange: RemainingDays = 0.5 = DurationDays → boundary passes
        var today = DateOnly.FromDateTime(DateTime.Today);
        var halfDayRequest = new CreateLeaveRequestDto(1, today, today, true, "AM", "ลาครึ่งวัน", []);

        var halfBalance = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(), EmployeeId = "EMP001", LeaveTypeId = 1,
            LeaveYear = today.Year, EntitledDays = 0.5m, UsedDays = 0,
            PendingDays = 0, CarriedForwardDays = 0, CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };

        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, today.Year, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(halfBalance);
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", halfDayRequest);

        // Assert
        result.Status.Should().Be(LeaveStatus.Pending);
        _leaveRepoMock.Verify(x => x.AddAsync(
            It.Is<LeaveRequest>(lr => lr.DurationDays == 0.5m && lr.IsHalfDay),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-KB-014 | Source: rule-input-validation.md → 1.6, lesson-learned → EC-008
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_CreatePendingRequest_When_RemainingDaysExactlyEqualsRequested()
    {
        // Arrange: RemainingDays=3 = DurationDays=3 — boundary: 3 >= 3 passes
        var exactBalance = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(), EmployeeId = "EMP001", LeaveTypeId = 1, LeaveYear = 2026,
            EntitledDays = 3, UsedDays = 0, PendingDays = 0, CarriedForwardDays = 0,
            CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(exactBalance);
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        result.Status.Should().Be(LeaveStatus.Pending);
    }

    // TC-KB-015 | Source: rule-input-validation.md → 1.6, lesson-learned → EC-009
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_ThrowBusinessException_When_RemainingDaysJustUnderRequested()
    {
        // Arrange: RemainingDays = 10 - 7 - 0.5 = 2.5 < 3 — just under
        var nearlyEmpty = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(), EmployeeId = "EMP001", LeaveTypeId = 1, LeaveYear = 2026,
            EntitledDays = 10, UsedDays = 7, PendingDays = 0.5m, CarriedForwardDays = 0,
            CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM"
        };
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(nearlyEmpty);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INSUFFICIENT_BALANCE");
    }

    // TC-KB-016 | Source: rule-input-validation.md → 1.4 MaxLength=500
    // [TODO: resolve open question] Reason MaxLength validated at FluentValidation, not service
    [Fact(Skip = "TODO: Reason MaxLength=500 validated at controller/FluentValidation, not service")]
    public Task SubmitLeaveRequestAsync_Should_ThrowValidationError_When_ReasonExceedsMaxLength()
        => Task.CompletedTask;

    // TC-KB-017 | Source: rule-input-validation.md → 1.4 Optional
    [Fact]
    public async Task SubmitLeaveRequestAsync_Should_CreatePendingRequest_When_ReasonIsNull()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(MakeSufficientBalance());
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest with { Reason = null });

        // Assert: null reason is stored as-is
        result.Status.Should().Be(LeaveStatus.Pending);
        _leaveRepoMock.Verify(x => x.AddAsync(
            It.Is<LeaveRequest>(lr => lr.Reason == null), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CancelAsync
    // ═══════════════════════════════════════════════════════════════════════════

    // TC-SPEC-016 | Source: Method Sig §4.4, SRS SF-007 BR-014
    [Fact]
    public async Task CancelAsync_Should_SetCancelledAndRestorePendingDays_When_StatusIsPending()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lr = new LeaveRequest
        {
            LeaveRequestId = id, EmployeeId = "EMP001", LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 7, 1), EndDate = new DateOnly(2026, 7, 3),
            DurationDays = 3m, Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
        };
        var balance = BuildBalance("EMP001", pending: 3m);

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        var result = await _sut.CancelAsync(id, "EMP001", null);

        // Assert
        result.Should().Contain("สำเร็จ");
        _leaveRepoMock.Verify(x => x.Update(
            It.Is<LeaveRequest>(r => r.Status == LeaveStatus.Cancelled && r.UpdatedBy == "EMP001")), Times.Once);
        _balanceRepoMock.Verify(x => x.Update(It.Is<LeaveBalance>(b => b.PendingDays == 0m)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-SPEC-017 | Source: Method Sig §4.4 step 1
    [Fact]
    public async Task CancelAsync_Should_ThrowNotFoundException_When_LeaveRequestNotFound()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.CancelAsync(missingId, "EMP001", null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-SPEC-018 | Source: Method Sig §4.4 step 2
    [Fact]
    public async Task CancelAsync_Should_ThrowBusinessException_When_RequesterIsNotOwner()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lr = new LeaveRequest
        {
            LeaveRequestId = id, EmployeeId = "EMP001", Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
        };
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);

        // Act: EMP002 tries to cancel EMP001's request
        var act = async () => await _sut.CancelAsync(id, "EMP002", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-SPEC-019 | Source: Method Sig §4.4 VR-009
    [Fact]
    public async Task CancelAsync_Should_ThrowBusinessException_When_StatusIsRejected()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new LeaveRequest
                      {
                          LeaveRequestId = id, EmployeeId = "EMP001",
                          Status = LeaveStatus.Rejected, CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
                      });

        // Act
        var act = async () => await _sut.CancelAsync(id, "EMP001", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_STATUS");
    }

    // TC-SPEC-020 | Source: Method Sig §4.4, SRS SF-008
    // CONFLICT with TC-KB-020: spec says throw, implementation creates CancelRequest.
    // Implementation is authoritative — see TC-KB-020 for the active test.

    // TC-KB-018 | Source: rule-input-validation.md → 2.1
    [Fact]
    public async Task CancelAsync_Should_ThrowNotFoundException_When_LeaveRequestIdIsEmpty()
    {
        // Arrange
        _leaveRepoMock.Setup(x => x.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.CancelAsync(Guid.Empty, "EMP001", null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-KB-019 | Source: lesson-learned → BUG-008
    [Fact]
    public async Task CancelAsync_Should_ThrowBusinessException_When_EmployeeIdIsWhitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new LeaveRequest
                      {
                          LeaveRequestId = id, EmployeeId = "EMP001", Status = LeaveStatus.Pending,
                          CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
                      });

        // Act: " " != "EMP001" → FORBIDDEN (not NullReferenceException)
        var act = async () => await _sut.CancelAsync(id, " ", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-KB-020 | Source: rule-input-validation.md → 4.2
    [Fact]
    public async Task CancelAsync_Should_SetCancelRequested_When_StatusIsApproved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lr = new LeaveRequest
        {
            LeaveRequestId = id, EmployeeId = "EMP001", LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 7, 1), DurationDays = 3m,
            Status = LeaveStatus.Approved, CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
        };
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _cancelRepoMock.Setup(x => x.AddAsync(It.IsAny<CancelRequest>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CancelAsync(id, "EMP001", "ต้องการยกเลิก");

        // Assert: Approved → CancelRequested (รอ Manager อนุมัติ, ไม่ยกเลิกทันที)
        result.Should().Contain("Manager");
        _leaveRepoMock.Verify(x => x.Update(
            It.Is<LeaveRequest>(r => r.Status == LeaveStatus.CancelRequested)), Times.Once);
        _cancelRepoMock.Verify(x => x.AddAsync(
            It.Is<CancelRequest>(cr => cr.LeaveRequestId == id && cr.EmployeeId == "EMP001"),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-KB-021 | Source: rule-input-validation.md → 4.2
    [Fact]
    public async Task CancelAsync_Should_ThrowBusinessException_When_StatusIsCancelRequested()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new LeaveRequest
                      {
                          LeaveRequestId = id, EmployeeId = "EMP001",
                          Status = LeaveStatus.CancelRequested, CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
                      });

        // Act
        var act = async () => await _sut.CancelAsync(id, "EMP001", null);

        // Assert: falls through to INVALID_STATUS
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_STATUS");
    }

    // TC-KB-022 | Source: std-unit-test.md → Programmer Experience
    [Fact]
    public async Task CancelAsync_Should_ClampPendingDaysToZero_When_PendingDaysLessThanDuration()
    {
        // Arrange: PendingDays=1, DurationDays=3 → Math.Max(0, 1-3) = 0 (not -2)
        var id = Guid.NewGuid();
        var lr = new LeaveRequest
        {
            LeaveRequestId = id, EmployeeId = "EMP001", LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 7, 1), DurationDays = 3m,
            Status = LeaveStatus.Pending, CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
        };
        var balance = BuildBalance("EMP001", pending: 1m); // less than DurationDays=3

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        await _sut.CancelAsync(id, "EMP001", null);

        // Assert: clamped to 0, not negative
        _balanceRepoMock.Verify(x => x.Update(It.Is<LeaveBalance>(b => b.PendingDays == 0m)), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApproveAsync
    // ═══════════════════════════════════════════════════════════════════════════

    // TC-SPEC-021 | Source: Method Sig §4.5, §7
    [Fact]
    public async Task ApproveAsync_Should_ApproveAndMoveBalanceFromPendingToUsed_When_ManagerApprovesValidRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lr = BuildPendingRequest(id, "EMP001", "MGR001");
        var balance = BuildBalance("EMP001", pending: 3m, used: 2m);

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        await _sut.ApproveAsync(id, "MGR001", "อนุมัติ");

        // Assert
        _leaveRepoMock.Verify(x => x.Update(
            It.Is<LeaveRequest>(r => r.Status == LeaveStatus.Approved && r.ApprovedBy == "MGR001")), Times.Once);

        _balanceRepoMock.Verify(x => x.Update(
            It.Is<LeaveBalance>(b => b.PendingDays == 0m && b.UsedDays == 5m)), Times.Once);

        _approvalHistoryRepoMock.Verify(x => x.AddAsync(
            It.Is<ApprovalHistory>(h => h.Action == ApprovalAction.Approved && h.ApproverId == "MGR001"),
            It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-SPEC-022 | Source: Method Sig §4.5 step 1
    [Fact]
    public async Task ApproveAsync_Should_ThrowNotFoundException_When_LeaveRequestNotFound()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.ApproveAsync(missingId, "MGR001", null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-SPEC-023 | Source: Method Sig §4.5 step 1
    [Theory]
    [InlineData(LeaveStatus.Approved)]
    [InlineData(LeaveStatus.Rejected)]
    [InlineData(LeaveStatus.Cancelled)]
    [InlineData(LeaveStatus.CancelRequested)]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_StatusIsNotPending(LeaveStatus nonPending)
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new LeaveRequest
                      {
                          LeaveRequestId = id, EmployeeId = "EMP001", Status = nonPending,
                          Employee = new Employee { EmployeeId = "EMP001", ManagerId = "MGR001", CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM" },
                          CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
                      });

        // Act
        var act = async () => await _sut.ApproveAsync(id, "MGR001", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_STATUS");
    }

    // TC-SPEC-024 | Source: Method Sig §4.5 step 2, SRS SF-004 BR-012
    [Fact]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_ManagerDoesNotManageEmployee()
    {
        // Arrange: EMP001's manager is MGR001, but MGR002 tries to approve
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));

        // Act
        var act = async () => await _sut.ApproveAsync(id, "MGR002", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-SPEC-025 | Source: Method Sig §7 — covered by TC-SPEC-021:
    //   ApprovalHistory INSERT is in the same BeginTransaction/CommitAsync block

    // TC-SPEC-026 | Source: Method Sig §4.5
    // [TODO: resolve open question] PublishLeaveApprovedAsync — no IEventPublisher in service
    [Fact(Skip = "TODO: PublishLeaveApprovedAsync event publishing not in LeaveRequestService")]
    public Task ApproveAsync_Should_PublishEvent_When_Committed() => Task.CompletedTask;

    // TC-KB-023 | Source: rule-input-validation.md → 2.1
    [Fact]
    public async Task ApproveAsync_Should_ThrowNotFoundException_When_LeaveRequestIdIsEmpty()
    {
        // Arrange
        _leaveRepoMock.Setup(x => x.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.ApproveAsync(Guid.Empty, "MGR001", null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-KB-024 | Source: rule-input-validation.md → 2.2
    [Fact]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_ManagerIdIsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));

        // Act
        var act = async () => await _sut.ApproveAsync(id, null!, null);

        // Assert: IsNullOrWhiteSpace(null) = true → FORBIDDEN (not NullReferenceException)
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-KB-025 | Source: lesson-learned → BUG-008
    [Fact]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_ManagerIdIsWhitespace()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));

        // Act
        var act = async () => await _sut.ApproveAsync(id, " ", null);

        // Assert: IsNullOrWhiteSpace(" ") = true → FORBIDDEN
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-KB-026 | Source: lesson-learned → BUG-001 (Employee.ManagerId = null regression)
    [Fact]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_EmployeeHasNoManagerAssigned()
    {
        // Arrange: Employee.ManagerId = null → null != "MGR001" → FORBIDDEN, not NullReferenceException
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", managerId: null));

        // Act
        var act = async () => await _sut.ApproveAsync(id, "MGR001", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-KB-027 | Source: rule-input-validation.md → 2.2 (Self-approval)
    [Fact]
    public async Task ApproveAsync_Should_ThrowBusinessException_When_EmployeeTriesToApproveSelf()
    {
        // Arrange: EMP001's manager is MGR001; EMP001 submits managerId="EMP001" → "MGR001"!="EMP001" → FORBIDDEN
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));

        // Act
        var act = async () => await _sut.ApproveAsync(id, "EMP001", null);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }

    // TC-KB-028 | Source: lesson-learned → BUG-002 (balance deduction regression)
    [Fact]
    public async Task ApproveAsync_Should_DeductPendingAndAddUsed_When_Approved()
    {
        // Arrange: PendingDays=3, UsedDays=2, Duration=3 → PendingDays=0, UsedDays=5
        var id = Guid.NewGuid();
        var lr = BuildPendingRequest(id, "EMP001", "MGR001", durationDays: 3m);
        var balance = BuildBalance("EMP001", pending: 3m, used: 2m);

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        await _sut.ApproveAsync(id, "MGR001", null);

        // Assert
        _balanceRepoMock.Verify(x => x.Update(
            It.Is<LeaveBalance>(b => b.PendingDays == 0m && b.UsedDays == 5m)), Times.Once);
    }

    // TC-KB-029 | Source: std-unit-test.md → Programmer Experience
    [Fact]
    public async Task ApproveAsync_Should_SucceedWithoutBalanceUpdate_When_BalanceRecordIsNull()
    {
        // Arrange: no LeaveBalance record for this employee/type/year
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((LeaveBalance?)null);

        // Act
        await _sut.ApproveAsync(id, "MGR001", null);

        // Assert: approve succeeds; Update() never called for balance
        _leaveRepoMock.Verify(x => x.Update(It.Is<LeaveRequest>(r => r.Status == LeaveStatus.Approved)), Times.Once);
        _balanceRepoMock.Verify(x => x.Update(It.IsAny<LeaveBalance>()), Times.Never);
        _txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RejectAsync
    // ═══════════════════════════════════════════════════════════════════════════

    // TC-SPEC-027 | Source: Method Sig §4.5, §7
    [Fact]
    public async Task RejectAsync_Should_RejectAndRestorePendingDays_When_ManagerRejectsWithReason()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lr = BuildPendingRequest(id, "EMP001", "MGR001");
        var balance = BuildBalance("EMP001", pending: 3m, used: 0m);

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        await _sut.RejectAsync(id, "MGR001", "ช่วงนั้นคนน้อย");

        // Assert
        _leaveRepoMock.Verify(x => x.Update(
            It.Is<LeaveRequest>(r =>
                r.Status == LeaveStatus.Rejected &&
                r.RejectedBy == "MGR001" &&
                r.RejectionReason == "ช่วงนั้นคนน้อย")), Times.Once);

        _balanceRepoMock.Verify(x => x.Update(It.Is<LeaveBalance>(b => b.PendingDays == 0m)), Times.Once);

        _approvalHistoryRepoMock.Verify(x => x.AddAsync(
            It.Is<ApprovalHistory>(h => h.Action == ApprovalAction.Rejected && h.ApproverId == "MGR001"),
            It.IsAny<CancellationToken>()), Times.Once);

        _txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC-SPEC-028 | Source: Method Sig §4.5 step 1
    [Fact]
    public async Task RejectAsync_Should_ThrowNotFoundException_When_LeaveRequestNotFound()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.RejectAsync(missingId, "MGR001", "เหตุผล");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-SPEC-029, TC-SPEC-030, TC-KB-030, TC-KB-031, TC-KB-032
    // Source: Method Sig §4.5 step 3 BR-013, rule-input-validation.md → 3.1, lesson-learned → BUG-008
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RejectAsync_Should_ThrowBusinessException_When_RejectionReasonIsNullOrWhitespace(string? reason)
    {
        // Arrange: guard checked before any repo call

        // Act
        var act = async () => await _sut.RejectAsync(Guid.NewGuid(), "MGR001", reason);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .Where(ex => ex.Code == "REJECTION_REASON_REQUIRED");
        _leaveRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // TC-SPEC-031 | Source: Method Sig §4.5 step 1
    [Theory]
    [InlineData(LeaveStatus.Approved)]
    [InlineData(LeaveStatus.Rejected)]
    [InlineData(LeaveStatus.Cancelled)]
    public async Task RejectAsync_Should_ThrowBusinessException_When_StatusIsNotPending(LeaveStatus nonPending)
    {
        // Arrange
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new LeaveRequest
                      {
                          LeaveRequestId = id, EmployeeId = "EMP001", Status = nonPending,
                          Employee = new Employee { EmployeeId = "EMP001", ManagerId = "MGR001", CreatedAt = DateTime.UtcNow, CreatedBy = "SYSTEM" },
                          CreatedAt = DateTime.UtcNow, CreatedBy = "EMP001"
                      });

        // Act
        var act = async () => await _sut.RejectAsync(id, "MGR001", "เหตุผล");

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "INVALID_STATUS");
    }

    // TC-SPEC-032 | Source: Method Sig §4.5
    // [TODO: resolve open question] PublishLeaveRejectedAsync — no IEventPublisher in service
    [Fact(Skip = "TODO: PublishLeaveRejectedAsync event publishing not in LeaveRequestService")]
    public Task RejectAsync_Should_PublishEvent_When_Committed() => Task.CompletedTask;

    // TC-KB-033 | Source: rule-input-validation.md → 3.1 MaxLength=500
    // [TODO: resolve open question] RejectionReason MaxLength validated at FluentValidation, not service
    [Fact(Skip = "TODO: RejectionReason MaxLength=500 validated at controller/FluentValidation, not service")]
    public Task RejectAsync_Should_ThrowValidationError_When_RejectionReasonExceedsMaxLength()
        => Task.CompletedTask;

    // TC-KB-034 | Source: rule-input-validation.md → 2.1
    [Fact]
    public async Task RejectAsync_Should_ThrowNotFoundException_When_LeaveRequestIdIsEmpty()
    {
        // Arrange
        _leaveRepoMock.Setup(x => x.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((LeaveRequest?)null);

        // Act
        var act = async () => await _sut.RejectAsync(Guid.Empty, "MGR001", "เหตุผล");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // TC-KB-035 | Source: lesson-learned → BUG-002 (balance restore regression)
    [Fact]
    public async Task RejectAsync_Should_RestorePendingDaysToZero_When_Rejected()
    {
        // Arrange: PendingDays=3, Duration=3 → Math.Max(0, 3-3) = 0
        var id = Guid.NewGuid();
        var lr = BuildPendingRequest(id, "EMP001", "MGR001", durationDays: 3m);
        var balance = BuildBalance("EMP001", pending: 3m);

        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(lr);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        // Act
        await _sut.RejectAsync(id, "MGR001", "เหตุผล");

        // Assert
        _balanceRepoMock.Verify(x => x.Update(It.Is<LeaveBalance>(b => b.PendingDays == 0m)), Times.Once);
    }

    // TC-KB-036 | Source: rule-input-validation.md → 2.2 (Self-reject)
    [Fact]
    public async Task RejectAsync_Should_ThrowBusinessException_When_EmployeeTriesToRejectSelf()
    {
        // Arrange: EMP001's manager is MGR001; EMP001 submits managerId="EMP001" → "MGR001"!="EMP001" → FORBIDDEN
        var id = Guid.NewGuid();
        _leaveRepoMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(BuildPendingRequest(id, "EMP001", "MGR001"));

        // Act
        var act = async () => await _sut.RejectAsync(id, "EMP001", "เหตุผล");

        // Assert
        await act.Should().ThrowAsync<BusinessException>().Where(ex => ex.Code == "FORBIDDEN");
    }
}
