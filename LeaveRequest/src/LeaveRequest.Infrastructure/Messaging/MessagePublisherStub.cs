namespace LeaveRequest.Infrastructure.Messaging;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Dev/test stub — logs the CloudEvent type without sending to Service Bus.
/// Activated via appsettings: "ServiceBus": { "UseStub": true }
/// </summary>
public sealed class MessagePublisherStub : IMessagePublisher
{
    private readonly ILogger<MessagePublisherStub> _logger;

    public MessagePublisherStub(ILogger<MessagePublisherStub> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(CloudEventDto cloudEvent, CancellationToken ct = default)
    {
        // ห้าม log body — PII
        _logger.LogInformation(
            "[STUB] IF-002 would publish {EventType}. MessageId={Id} CorrelationId={CorrelationId}",
            cloudEvent.Type, cloudEvent.Id, cloudEvent.CorrelationId);

        return Task.CompletedTask;
    }
}
