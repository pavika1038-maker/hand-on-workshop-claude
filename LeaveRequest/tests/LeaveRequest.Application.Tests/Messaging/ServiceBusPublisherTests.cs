namespace LeaveRequest.Application.Tests.Messaging;

using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ServiceBusPublisherTests
{
    // ── Fixture ────────────────────────────────────────────────────────────────

    private static CloudEventDto SampleEvent(string type = "com.abccompany.leave.request.submitted") => new(
        SpecVersion:     "1.0",
        Type:            type,
        Source:          "/leave-service/api/v1",
        Id:              Guid.NewGuid().ToString(),
        Time:            DateTime.UtcNow,
        DataContentType: "application/json",
        CorrelationId:   Guid.NewGuid().ToString(),
        Data:            new { test = "data" }
    );

    private static ServiceBusPublisher BuildSut(
        Func<Task> sendBehavior,
        out List<string> capturedMessageIds)
    {
        var ids = new List<string>();
        capturedMessageIds = ids;

        return new ServiceBusPublisher(
            sendAsync: (msg, _) =>
            {
                ids.Add(msg.MessageId);
                return sendBehavior();
            },
            NullLogger<ServiceBusPublisher>.Instance);
    }

    // ── Test 1: Publish success ────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_HappyPath_SendsMessageAndSetsCorrelationId()
    {
        List<string> capturedMessageIds;
        string? capturedCorrelationId = null;

        var sut = new ServiceBusPublisher(
            sendAsync: (msg, _) =>
            {
                capturedCorrelationId = msg.ApplicationProperties["correlationId"]?.ToString();
                capturedMessageIds = [msg.MessageId];
                return Task.CompletedTask;
            },
            NullLogger<ServiceBusPublisher>.Instance);

        var ev = SampleEvent();
        await sut.PublishAsync(ev);

        capturedCorrelationId.Should().Be(ev.CorrelationId);
    }

    // ── Test 2: Transient error → Polly retries → eventually succeeds ──────────

    [Fact]
    public async Task PublishAsync_TransientFailureThenSuccess_RetriesAndSucceeds()
    {
        var callCount = 0;
        var sut = new ServiceBusPublisher(
            sendAsync: (_, _) =>
            {
                callCount++;
                // Fail first two attempts, succeed on third
                if (callCount < 3)
                    throw new TimeoutException("transient");
                return Task.CompletedTask;
            },
            NullLogger<ServiceBusPublisher>.Instance);

        var act = async () => await sut.PublishAsync(SampleEvent());

        // Should not throw — Polly absorbed the retries
        await act.Should().NotThrowAsync();
        callCount.Should().Be(3);
    }

    // ── Test 3: All retries exhausted → MessagePublishException ───────────────

    [Fact]
    public async Task PublishAsync_AllRetriesExhausted_ThrowsMessagePublishException()
    {
        var callCount = 0;
        var sut = new ServiceBusPublisher(
            sendAsync: (_, _) =>
            {
                callCount++;
                throw new TimeoutException("always fails");
            },
            NullLogger<ServiceBusPublisher>.Instance);

        var ev = SampleEvent();
        var act = async () => await sut.PublishAsync(ev);

        var ex = await act.Should().ThrowAsync<MessagePublishException>();
        ex.Which.EventType.Should().Be(ev.Type);
        ex.Which.InnerException.Should().BeOfType<TimeoutException>();

        // 1 original attempt + 3 Polly retries
        callCount.Should().Be(4);
    }

    // ── Test 4: MessageId = CloudEvent.Id ─────────────────────────────────────

    [Fact]
    public async Task PublishAsync_SetsMessageIdToCloudEventId()
    {
        string? capturedId = null;
        var sut = new ServiceBusPublisher(
            sendAsync: (msg, _) => { capturedId = msg.MessageId; return Task.CompletedTask; },
            NullLogger<ServiceBusPublisher>.Instance);

        var ev = SampleEvent();
        await sut.PublishAsync(ev);

        capturedId.Should().Be(ev.Id);
    }
}
