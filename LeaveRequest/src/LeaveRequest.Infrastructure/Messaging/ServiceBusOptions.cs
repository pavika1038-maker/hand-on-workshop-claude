namespace LeaveRequest.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public bool UseStub { get; set; } = true;

    // Azure Service Bus fully-qualified namespace, e.g. "mynamespace.servicebus.windows.net"
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    // Connection string — used as alternative to managed identity
    public string? ConnectionString { get; set; }

    public string TopicName { get; set; } = "leave-events";
    public string SubscriptionName { get; set; } = "email-notify";

    // Maximum concurrent message processing
    public int MaxConcurrentMessages { get; set; } = 5;
}
