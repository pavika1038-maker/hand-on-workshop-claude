namespace LeaveRequest.Application.Tests.Services;

using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Services;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using Moq;
using LeaveRequestEntity = global::LeaveRequest.Domain.Entities.LeaveRequest;

public class LeaveReportServiceTests
{
    private readonly Mock<ILeaveReportRepository> _repoMock = new();
    private readonly LeaveReportService _sut;

    public LeaveReportServiceTests()
    {
        _sut = new LeaveReportService(_repoMock.Object);
    }

    private static LeaveRequestEntity MakeLeaveRequest(
        string employeeId = "EMP001",
        byte leaveTypeId = 1,
        LeaveStatus status = LeaveStatus.Approved,
        string department = "IT",
        EmployeeType employeeType = EmployeeType.Regular,
        DateOnly? start = null,
        DateOnly? end = null)
    {
        return new LeaveRequestEntity
        {
            LeaveRequestId = Guid.NewGuid(),
            LeaveRequestRef = "LR-2026-0001",
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            StartDate = start ?? new DateOnly(2026, 1, 6),
            EndDate = end ?? new DateOnly(2026, 1, 8),
            DurationDays = 3,
            IsHalfDay = false,
            HalfDayPeriod = null,
            Status = status,
            ApprovedBy = status == LeaveStatus.Approved ? "MGR001" : null,
            ApprovedAt = status == LeaveStatus.Approved ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            Employee = new Employee
            {
                EmployeeId = employeeId,
                FullNameTh = "ทดสอบ ระบบ",
                FullNameEn = "Test User",
                Department = department,
                EmployeeType = employeeType
            },
            LeaveType = new LeaveType
            {
                LeaveTypeId = leaveTypeId,
                TypeCode = "ANNUAL",
                TypeNameTh = "ลาพักร้อน",
                TypeNameEn = "Annual Leave"
            }
        };
    }

    // ── Happy Path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_WhenDataExists_ReturnsMappedPagedResult()
    {
        // Arrange
        var entities = new List<LeaveRequestEntity> { MakeLeaveRequest() };
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 1));

        // Act
        var result = await _sut.GetLeaveHistoryAsync(new LeaveHistoryFilterRequest());

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);

        var item = result.Items[0];
        item.EmployeeId.Should().Be("EMP001");
        item.EmployeeFullNameTh.Should().Be("ทดสอบ ระบบ");
        item.LeaveTypeNameTh.Should().Be("ลาพักร้อน");
        item.Status.Should().Be(LeaveStatus.Approved);
        item.DurationDays.Should().Be(3);
    }

    // ── Empty Result ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_WhenNoData_ReturnsEmptyPagedResult()
    {
        // Arrange
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        // Act
        var result = await _sut.GetLeaveHistoryAsync(new LeaveHistoryFilterRequest());

        // Assert
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }

    // ── Pagination ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_Pagination_TotalPagesIsCorrect()
    {
        // Arrange — 23 records with pageSize=5 → 5 pages
        var items = Enumerable.Range(0, 5).Select(_ => MakeLeaveRequest()).ToList();
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 23));

        var filter = new LeaveHistoryFilterRequest { Page = 2, PageSize = 5 };

        // Act
        var result = await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(23);
        result.TotalPages.Should().Be(5); // ceil(23/5) = 5
    }

    [Fact]
    public async Task GetLeaveHistoryAsync_Pagination_PassesPageAndPageSizeToRepo()
    {
        // Arrange
        int capturedPage = 0;
        int capturedPageSize = 0;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((_, p, ps, _) =>
            {
                capturedPage = p;
                capturedPageSize = ps;
            })
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest { Page = 3, PageSize = 10 };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        capturedPage.Should().Be(3);
        capturedPageSize.Should().Be(10);
    }

    // ── Filter: EmployeeId ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_FilterByEmployeeId_PassesEmployeeIdToQuery()
    {
        // Arrange
        LeaveHistoryQuery? captured = null;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((q, _, _, _) => captured = q)
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest { EmployeeId = "EMP099" };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        captured.Should().NotBeNull();
        captured!.EmployeeId.Should().Be("EMP099");
    }

    // ── Filter: Status ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_FilterByStatus_PassesStatusToQuery()
    {
        // Arrange
        LeaveHistoryQuery? captured = null;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((q, _, _, _) => captured = q)
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest { Status = LeaveStatus.Pending };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        captured!.Status.Should().Be(LeaveStatus.Pending);
    }

    // ── Filter: DateRange ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_FilterByDateRange_PassesDatesToQuery()
    {
        // Arrange
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        LeaveHistoryQuery? captured = null;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((q, _, _, _) => captured = q)
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest { StartDate = start, EndDate = end };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        captured!.StartDate.Should().Be(start);
        captured!.EndDate.Should().Be(end);
    }

    // ── Filter: LeaveType ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_FilterByLeaveTypeId_PassesLeaveTypeIdToQuery()
    {
        // Arrange
        LeaveHistoryQuery? captured = null;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((q, _, _, _) => captured = q)
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest { LeaveTypeId = 2 };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        captured!.LeaveTypeId.Should().Be(2);
    }

    // ── Filter: Department + EmployeeType ────────────────────────────────────────

    [Fact]
    public async Task GetLeaveHistoryAsync_FilterByDepartmentAndEmployeeType_PassesBothToQuery()
    {
        // Arrange
        LeaveHistoryQuery? captured = null;
        _repoMock.Setup(x => x.GetLeaveHistoryAsync(
                It.IsAny<LeaveHistoryQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<LeaveHistoryQuery, int, int, CancellationToken>((q, _, _, _) => captured = q)
            .ReturnsAsync((new List<LeaveRequestEntity>(), 0));

        var filter = new LeaveHistoryFilterRequest
        {
            Department = "Finance",
            EmployeeType = EmployeeType.Outsource
        };

        // Act
        await _sut.GetLeaveHistoryAsync(filter);

        // Assert
        captured!.Department.Should().Be("Finance");
        captured!.EmployeeType.Should().Be(EmployeeType.Outsource);
    }
}
