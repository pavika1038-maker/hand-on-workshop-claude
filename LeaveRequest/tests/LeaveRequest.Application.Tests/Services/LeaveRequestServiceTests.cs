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
    private readonly Mock<ILeaveRequestRepository> _leaveRepoMock = new();
    private readonly Mock<ILeaveBalanceRepository> _balanceRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<ILeaveTypeRepository> _leaveTypeRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ITransaction> _txMock = new();
    private readonly Mock<ILogger<LeaveRequestService>> _loggerMock = new();
    private readonly LeaveRequestService _sut;

    // ── Shared test data ────────────────────────────────────────────────────

    private static readonly Employee ActiveEmployee = new()
    {
        EmployeeId = "EMP001",
        EmployeeCode = "E001",
        FullNameTh = "สมชาย ใจดี",
        FullNameEn = "Somchai Jaidee",
        Email = "somchai@abc.com",
        HireDate = new DateOnly(2022, 1, 1),
        EmployeeType = EmployeeType.Regular,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "SYSTEM"
    };

    private static readonly LeaveType AnnualLeaveType = new()
    {
        LeaveTypeId = 1,
        TypeCode = "ANNUAL",
        TypeNameTh = "ลาพักผ่อน",
        TypeNameEn = "Annual Leave",
        MaxDaysPerYear = 10,
        IsAvailableForOutsource = false,
        RequiresMedicalCert = false,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "SYSTEM"
    };

    private static readonly LeaveBalance SufficientBalance = new()
    {
        LeaveBalanceId = Guid.NewGuid(),
        EmployeeId = "EMP001",
        LeaveTypeId = 1,
        LeaveYear = 2026,
        EntitledDays = 10,
        UsedDays = 2,
        PendingDays = 0,
        CarriedForwardDays = 0,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "SYSTEM"
    };

    private static readonly CreateLeaveRequestDto ValidRequest = new(
        LeaveTypeId: 1,
        StartDate: new DateOnly(2026, 7, 1),
        EndDate: new DateOnly(2026, 7, 3),
        IsHalfDay: false,
        HalfDayPeriod: null,
        Reason: "พักผ่อน",
        AttachmentIds: []
    );

    public LeaveRequestServiceTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        _uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_txMock.Object);
        _txMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        _txMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        _txMock.Setup(x => x.DisposeAsync())
               .Returns(ValueTask.CompletedTask);

        _sut = new LeaveRequestService(
            _leaveRepoMock.Object,
            _balanceRepoMock.Object,
            _employeeRepoMock.Object,
            _leaveTypeRepoMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    // ── Happy Path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequestAsync_HappyPath_CreatesRequestAndDeductsPending()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(SufficientBalance);
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([]);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        result.Status.Should().Be(LeaveStatus.Pending);
        result.LeaveRequestRef.Should().StartWith("LR-2026-");
        result.Message.Should().Contain("สำเร็จ");

        _leaveRepoMock.Verify(x =>
            x.AddAsync(
                It.Is<LeaveRequest>(lr =>
                    lr.EmployeeId == "EMP001" &&
                    lr.LeaveTypeId == 1 &&
                    lr.DurationDays == 3m &&
                    lr.Status == LeaveStatus.Pending),
                It.IsAny<CancellationToken>()), Times.Once);

        _balanceRepoMock.Verify(x => x.Update(
            It.Is<LeaveBalance>(b => b.PendingDays == 3m)), Times.Once);

        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── VR-002: Insufficient Balance ────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequestAsync_VR002_InsufficientBalance_ThrowsBusinessException()
    {
        // Arrange: remaining = 10 + 0 - 9 - 0 = 1 วัน, request = 3 วัน → fail
        var lowBalance = new LeaveBalance
        {
            LeaveBalanceId = Guid.NewGuid(),
            EmployeeId = "EMP001",
            LeaveTypeId = 1,
            LeaveYear = 2026,
            EntitledDays = 10,
            UsedDays = 9,
            PendingDays = 0,
            CarriedForwardDays = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
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
        await act.Should()
            .ThrowAsync<BusinessException>()
            .Where(ex => ex.Code == "INSUFFICIENT_BALANCE");

        // ไม่มีการ BeginTransaction หรือ SaveChanges เมื่อ validation fail
        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _leaveRepoMock.Verify(x => x.AddAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── VR-003: Date Conflict (Overlap) ────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequestAsync_VR003_DateConflict_ThrowsBusinessException()
    {
        // Arrange: existing Pending request overlaps with requested dates
        var existingRequest = new LeaveRequest
        {
            LeaveRequestId = Guid.NewGuid(),
            LeaveRequestRef = "LR-2026-AAAA1234",
            EmployeeId = "EMP001",
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 7, 2),
            EndDate = new DateOnly(2026, 7, 4),
            DurationDays = 3m,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "EMP001"
        };

        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(AnnualLeaveType);
        _balanceRepoMock.Setup(x => x.GetAsync("EMP001", 1, 2026, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(SufficientBalance);
        _leaveRepoMock.Setup(x => x.GetOverlappingAsync(
                "EMP001", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync([existingRequest]); // overlap found!

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", ValidRequest);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .Where(ex => ex.Code == "DATE_CONFLICT");

        _uowMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── NotFoundException: Employee ─────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequestAsync_EmployeeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP999", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Employee?)null);

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP999", ValidRequest);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*EMP999*");

        _leaveTypeRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<byte>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── NotFoundException: LeaveType ────────────────────────────────────────

    [Fact]
    public async Task SubmitLeaveRequestAsync_LeaveTypeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _employeeRepoMock.Setup(x => x.GetByIdAsync("EMP001", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ActiveEmployee);
        _leaveTypeRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((LeaveType?)null);

        var requestWithBadType = ValidRequest with { LeaveTypeId = 99 };

        // Act
        var act = async () => await _sut.SubmitLeaveRequestAsync("EMP001", requestWithBadType);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }
}
