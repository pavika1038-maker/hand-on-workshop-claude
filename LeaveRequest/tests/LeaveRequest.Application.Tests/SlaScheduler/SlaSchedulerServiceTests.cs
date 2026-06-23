namespace LeaveRequest.Application.Tests.SlaScheduler;

using FluentAssertions;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using LeaveRequest.Infrastructure.SlaScheduler;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

public sealed class SlaSchedulerServiceTests : IDisposable
{
    private static readonly DateTime CheckTime = new(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<ICancelRequestRepository> _cancelRepoMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();

    private readonly SlaSchedulerOptions _options = new()
    {
        IntervalMinutes   = 5,
        ReminderWindowHours = 4,
    };

    public SlaSchedulerServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(dbOptions);
        _context.Database.EnsureCreated();

        // Default: UpdateAsync no-op
        _cancelRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CancelRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Default: notifications no-op
        _notificationMock
            .Setup(n => n.PublishSlaReminderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _notificationMock
            .Setup(n => n.PublishSlaEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private SlaSchedulerService BuildSut() => new(
        _cancelRepoMock.Object,
        _notificationMock.Object,
        _context,
        _options,
        NullLogger<SlaSchedulerService>.Instance);

    private static CancelRequest MakeCancelRequest(
        Guid id, DateTime slaDeadline,
        DateTime? reminderSentAt = null,
        DateTime? escalatedAt = null,
        CancelRequestStatus status = CancelRequestStatus.Pending)
        => new()
        {
            CancelRequestId   = id,
            CancelRequestRef  = id.ToString("N")[..8],
            LeaveRequestId    = Guid.NewGuid(),
            EmployeeId        = "EMP001",
            Status            = status,
            SlaDeadline       = slaDeadline,
            SlaReminderSentAt = reminderSentAt,
            SlaEscalatedAt    = escalatedAt,
            CreatedAt         = slaDeadline.AddHours(-48),
            CreatedBy         = "test",
        };

    // ── Test 1: Normal run — one Reminder, one Escalation ─────────────────────

    [Fact]
    public async Task ProcessSlaEventsAsync_NormalRun_SendsReminderAndEscalation()
    {
        var reminderCrId = Guid.NewGuid();
        var escalateCrId = Guid.NewGuid();

        // Reminder window: SlaDeadline is 2h from now → within 4h window
        var reminderCr  = MakeCancelRequest(reminderCrId, CheckTime.AddHours(2));

        // Escalation: SlaDeadline already passed
        var escalateCr  = MakeCancelRequest(escalateCrId, CheckTime.AddMinutes(-30));

        _cancelRepoMock
            .Setup(r => r.GetPendingForSlaCheckAsync(CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { reminderCr, escalateCr });

        var sut = BuildSut();
        await sut.ProcessSlaEventsAsync(CheckTime);

        // Reminder sent for the one in window
        _notificationMock.Verify(n =>
            n.PublishSlaReminderAsync(reminderCrId, It.IsAny<CancellationToken>()),
            Times.Once);
        reminderCr.SlaReminderSentAt.Should().Be(CheckTime);

        // Escalation sent for the overdue one
        _notificationMock.Verify(n =>
            n.PublishSlaEscalatedAsync(escalateCrId, It.IsAny<CancellationToken>()),
            Times.Once);
        escalateCr.SlaEscalatedAt.Should().Be(CheckTime);
        escalateCr.Status.Should().Be(CancelRequestStatus.Escalated);

        _cancelRepoMock.Verify(r =>
            r.UpdateAsync(It.IsAny<CancelRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ── Test 2: No overdue records — nothing happens ──────────────────────────

    [Fact]
    public async Task ProcessSlaEventsAsync_NoOverdueRecords_NoNotificationsSent()
    {
        _cancelRepoMock
            .Setup(r => r.GetPendingForSlaCheckAsync(CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CancelRequest>());

        var sut = BuildSut();
        await sut.ProcessSlaEventsAsync(CheckTime);

        _notificationMock.Verify(n =>
            n.PublishSlaReminderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationMock.Verify(n =>
            n.PublishSlaEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _cancelRepoMock.Verify(r =>
            r.UpdateAsync(It.IsAny<CancelRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Test 3: Partial error — first record fails, second still processed ────

    [Fact]
    public async Task ProcessSlaEventsAsync_PartialError_SecondRecordStillProcessed()
    {
        var failCrId   = Guid.NewGuid();
        var okCrId     = Guid.NewGuid();

        var failCr = MakeCancelRequest(failCrId, CheckTime.AddMinutes(-10));
        var okCr   = MakeCancelRequest(okCrId,   CheckTime.AddMinutes(-5));

        _cancelRepoMock
            .Setup(r => r.GetPendingForSlaCheckAsync(CheckTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { failCr, okCr });

        // First record throws when trying to publish escalation
        _notificationMock
            .Setup(n => n.PublishSlaEscalatedAsync(failCrId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service Bus unavailable"));

        var sut = BuildSut();

        // Should NOT throw — errors are caught per record
        var act = async () => await sut.ProcessSlaEventsAsync(CheckTime);
        await act.Should().NotThrowAsync();

        // Second record should still be escalated
        _notificationMock.Verify(n =>
            n.PublishSlaEscalatedAsync(okCrId, It.IsAny<CancellationToken>()),
            Times.Once);
        okCr.SlaEscalatedAt.Should().Be(CheckTime);
    }

    // ── Test 4: Cancellation — ExecuteAsync exits cleanly ────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ExitsWithoutException()
    {
        using var cts = new CancellationTokenSource();

        _cancelRepoMock
            .Setup(r => r.GetPendingForSlaCheckAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CancelRequest>());

        var sut = BuildSut();

        // Cancel immediately after starting
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var act = async () => await sut.StartAsync(cts.Token)
            .ContinueWith(_ => sut.StopAsync(CancellationToken.None)).Unwrap();

        await act.Should().NotThrowAsync();
    }
}
