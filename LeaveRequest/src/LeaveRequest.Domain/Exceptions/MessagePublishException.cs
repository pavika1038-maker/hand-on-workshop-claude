namespace LeaveRequest.Domain.Exceptions;

public sealed class MessagePublishException : Exception
{
    public string EventType { get; }

    public MessagePublishException(string eventType, Exception? inner = null)
        : base($"Failed to publish event '{eventType}' to Service Bus.", inner)
    {
        EventType = eventType;
    }
}
