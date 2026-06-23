namespace LeaveRequest.Infrastructure.Messaging;

using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

public sealed class ServiceBusPublisher : IMessagePublisher
{
    // Thin delegate so tests can inject a fake sender without Azure SDK types
    private readonly Func<ServiceBusMessage, CancellationToken, Task> _sendAsync;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ILogger<ServiceBusPublisher> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    // Production constructor — DI injects the real ServiceBusSender
    public ServiceBusPublisher(
        ServiceBusSender sender,
        ILogger<ServiceBusPublisher> logger)
        : this((msg, ct) => sender.SendMessageAsync(msg, ct), logger)
    {
    }

    // Testable constructor — accepts a send-delegate (avoids Azure SDK coupling in tests)
    internal ServiceBusPublisher(
        Func<ServiceBusMessage, CancellationToken, Task> sendAsync,
        ILogger<ServiceBusPublisher> logger)
    {
        _sendAsync = sendAsync;
        _logger = logger;
        _retryPipeline = BuildRetryPipeline();
    }

    public async Task PublishAsync(CloudEventDto cloudEvent, CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(cloudEvent, SerializerOptions);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/cloudevents+json; charset=utf-8",
            MessageId = cloudEvent.Id,
            // correlationId in Application Properties — architecture §12.3
            ApplicationProperties = { ["correlationId"] = cloudEvent.CorrelationId }
        };

        // ห้าม log body — contains PII (employee name, email)
        _logger.LogInformation(
            "IF-002 Publishing {EventType}. MessageId={MessageId} CorrelationId={CorrelationId}",
            cloudEvent.Type, message.MessageId, cloudEvent.CorrelationId);

        try
        {
            await _retryPipeline.ExecuteAsync(async token =>
                await _sendAsync(message, token), ct);

            _logger.LogInformation(
                "IF-002 Published {EventType} successfully. MessageId={MessageId}",
                cloudEvent.Type, message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "IF-002 Publish failed after retries. EventType={EventType} MessageId={MessageId} CorrelationId={CorrelationId}",
                cloudEvent.Type, message.MessageId, cloudEvent.CorrelationId);

            throw new MessagePublishException(cloudEvent.Type, ex);
        }
    }

    private static ResiliencePipeline BuildRetryPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = false,
                ShouldHandle = new PredicateBuilder()
                    .Handle<ServiceBusException>(ex => ex.IsTransient)
                    .Handle<TimeoutException>()
                    .Handle<OperationCanceledException>(ex => ex is not TaskCanceledException)
            })
            .Build();
}
