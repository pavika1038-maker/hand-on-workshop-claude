namespace LeaveRequest.Infrastructure.Messaging;

using LeaveRequest.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Dev/test stub — logs incoming messages without actually sending email or calling SMTP.
/// Activated via appsettings: "ServiceBus": { "UseStub": true }
/// </summary>
public sealed class EmailConsumerStub : BackgroundService, IEmailConsumer
{
    private readonly ILogger<EmailConsumerStub> _logger;

    public EmailConsumerStub(ILogger<EmailConsumerStub> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[STUB] IF-002 EmailConsumerStub started — no Service Bus connection.");
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public Task ConsumeAsync(
        string messageBody,
        string messageId,
        Func<CancellationToken, Task> completeAsync,
        Func<CancellationToken, Task> abandonAsync,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[STUB] IF-002 ConsumeAsync called. MessageId={MessageId}", messageId);
        return completeAsync(ct);
    }

    public Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        // ห้าม log recipientEmail — PII
        _logger.LogInformation("[STUB] IF-002 SendEmailAsync called. Subject={Subject}", subject);
        return Task.CompletedTask;
    }
}
