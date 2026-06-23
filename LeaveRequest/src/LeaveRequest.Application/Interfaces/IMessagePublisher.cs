namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface IMessagePublisher
{
    // Publish CloudEvent to Azure Service Bus Topic "leave-events"
    // correlationId is forwarded as an Application Property on the message.
    // Throws MessagePublishException after Polly retry exhausted.
    Task PublishAsync(CloudEventDto cloudEvent, CancellationToken ct = default);
}
