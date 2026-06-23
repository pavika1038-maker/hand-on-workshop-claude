namespace LeaveRequest.Application.Tests.Messaging;

using System.Text.Json;
using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

public sealed class EmailConsumerTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────

    private static readonly Guid LogId = Guid.NewGuid();
    private const string MessageId = "test-msg-001";

    private static string BuildMessageBody(Guid notificationLogId, string eventType = "LeaveSubmitted")
    {
        var data = new LeaveNotificationData(
            NotificationLogId: notificationLogId,
            EventType:         eventType,
            LeaveRequestId:    Guid.NewGuid(),
            CancelRequestId:   null,
            EmployeeId:        "EMP001",
            EmployeeName:      "สมชาย ใจดี",
            LeaveTypeName:     "ลาพักผ่อน",
            StartDate:         new DateOnly(2026, 7, 1),
            EndDate:           new DateOnly(2026, 7, 3),
            DurationDays:      3m,
            RejectionReason:   null,
            Recipients:        [new("manager@abc.com", "Manager"), new("hr@abc.com", "HR")]
        );

        var envelope = new CloudEventDto(
            SpecVersion:     "1.0",
            Type:            "com.abccompany.leave.request.submitted",
            Source:          "/leave-service",
            Id:              Guid.NewGuid().ToString(),
            Time:            DateTime.UtcNow,
            DataContentType: "application/json",
            CorrelationId:   Guid.NewGuid().ToString(),
            Data:            data
        );

        return JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static TestableEmailConsumer BuildSut(
        Mock<INotificationLogRepository> repoMock,
        Func<string, string, string, CancellationToken, Task>? sendOverride = null)
    {
        var smtpOpts = Options.Create(new SmtpOptions { Host = "localhost", Port = 25, EnableSsl = false });
        var sbOpts   = Options.Create(new ServiceBusOptions());

        return new TestableEmailConsumer(
            repoMock.Object,
            smtpOpts,
            sbOpts,
            NullLogger<EmailConsumer>.Instance,
            sendOverride ?? ((_, _, _, _) => Task.CompletedTask));
    }

    // ── Test 1: Happy path — email sent + log updated + ACK ──────────────────

    [Fact]
    public async Task ConsumeAsync_HappyPath_SendsEmailAndCompletesMessage()
    {
        var repoMock = new Mock<INotificationLogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(LogId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationLog?)null);

        var emailsSent = new List<string>();
        var sut = BuildSut(repoMock, sendOverride: (to, _, _, _) =>
        {
            emailsSent.Add(to);
            return Task.CompletedTask;
        });

        var completed = false;
        var abandoned = false;

        await sut.ConsumeAsync(
            BuildMessageBody(LogId),
            MessageId,
            completeAsync: _ => { completed = true; return Task.CompletedTask; },
            abandonAsync:  _ => { abandoned = true; return Task.CompletedTask; });

        completed.Should().BeTrue();
        abandoned.Should().BeFalse();
        emailsSent.Should().HaveCount(2); // Manager + HR
        repoMock.Verify(r => r.UpdateDeliveryStatusAsync(
            LogId, DeliveryStatus.Success, It.IsAny<int>(), It.IsAny<DateTime?>(), null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 2: Idempotency — already delivered → ACK without re-sending ─────

    [Fact]
    public async Task ConsumeAsync_AlreadyDelivered_SkipsEmailAndAcks()
    {
        var repoMock = new Mock<INotificationLogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(LogId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotificationLog
                {
                    NotificationLogId = LogId,
                    Status = DeliveryStatus.Success
                });

        var emailsSent = new List<string>();
        var sut = BuildSut(repoMock, sendOverride: (to, _, _, _) =>
        {
            emailsSent.Add(to);
            return Task.CompletedTask;
        });

        var completed = false;
        await sut.ConsumeAsync(
            BuildMessageBody(LogId),
            MessageId,
            completeAsync: _ => { completed = true; return Task.CompletedTask; },
            abandonAsync:  _ => Task.CompletedTask);

        completed.Should().BeTrue();
        emailsSent.Should().BeEmpty("duplicate message must not re-send email");
        repoMock.Verify(r => r.UpdateDeliveryStatusAsync(
            It.IsAny<Guid>(), It.IsAny<DeliveryStatus>(), It.IsAny<int>(),
            It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Test 3: SMTP failure → abandon (DLQ path) ────────────────────────────

    [Fact]
    public async Task ConsumeAsync_SmtpFails_UpdatesLogToFailedAndAbandons()
    {
        var repoMock = new Mock<INotificationLogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(LogId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationLog?)null);

        var sut = BuildSut(repoMock, sendOverride: (_, _, _, _) =>
            throw new InvalidOperationException("SMTP connection refused"));

        var abandoned = false;
        var completed = false;

        await sut.ConsumeAsync(
            BuildMessageBody(LogId),
            MessageId,
            completeAsync: _ => { completed = true; return Task.CompletedTask; },
            abandonAsync:  _ => { abandoned = true; return Task.CompletedTask; });

        abandoned.Should().BeTrue("consumer must abandon — never throw — on failure");
        completed.Should().BeFalse();
        repoMock.Verify(r => r.UpdateDeliveryStatusAsync(
            LogId, DeliveryStatus.Failed, It.IsAny<int>(), null, It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 4: Malformed JSON → abandon without crashing ────────────────────

    [Fact]
    public async Task ConsumeAsync_MalformedJson_AbandonsWithoutException()
    {
        var repoMock = new Mock<INotificationLogRepository>();
        var sut = BuildSut(repoMock);

        var abandoned = false;
        var act = async () => await sut.ConsumeAsync(
            "{ this is not valid JSON }",
            MessageId,
            completeAsync: _ => Task.CompletedTask,
            abandonAsync:  _ => { abandoned = true; return Task.CompletedTask; });

        await act.Should().NotThrowAsync();
        abandoned.Should().BeTrue();
    }

    // ── TestableEmailConsumer (overrides SendEmailAsync for isolation) ─────────

    private sealed class TestableEmailConsumer : EmailConsumer
    {
        private readonly Func<string, string, string, CancellationToken, Task> _sendOverride;

        public TestableEmailConsumer(
            INotificationLogRepository repo,
            IOptions<SmtpOptions> smtpOpts,
            IOptions<ServiceBusOptions> sbOpts,
            Microsoft.Extensions.Logging.ILogger<EmailConsumer> logger,
            Func<string, string, string, CancellationToken, Task> sendOverride)
            // ServiceBusClient is null — only ConsumeAsync is tested here
            : base(repo, null!, smtpOpts, sbOpts, logger)
        {
            _sendOverride = sendOverride;
        }

        public override Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken ct = default)
            => _sendOverride(recipientEmail, subject, htmlBody, ct);
    }
}
