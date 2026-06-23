namespace LeaveRequest.Infrastructure.Messaging;

using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// IHostedService that consumes messages from Service Bus Subscription "email-notify".
/// Idempotency: skips messages whose NotificationLog is already Status=Success.
/// DLQ: abandons on error (never throws outside) — SB retries up to MaxDeliveryCount=10.
/// </summary>
public class EmailConsumer : BackgroundService, IEmailConsumer
{
    private readonly INotificationLogRepository _notificationLogRepo;
    private readonly SmtpOptions _smtpOptions;
    private readonly ServiceBusOptions _sbOptions;
    private readonly ServiceBusClient _sbClient;
    private readonly ILogger<EmailConsumer> _logger;

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmailConsumer(
        INotificationLogRepository notificationLogRepo,
        ServiceBusClient sbClient,
        IOptions<SmtpOptions> smtpOptions,
        IOptions<ServiceBusOptions> sbOptions,
        ILogger<EmailConsumer> logger)
    {
        _notificationLogRepo = notificationLogRepo;
        _sbClient = sbClient;
        _smtpOptions = smtpOptions.Value;
        _sbOptions = sbOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _sbOptions.MaxConcurrentMessages,
            AutoCompleteMessages = false
        };

        await using var processor = _sbClient.CreateProcessor(
            _sbOptions.TopicName,
            _sbOptions.SubscriptionName,
            processorOptions);

        processor.ProcessMessageAsync += OnMessageAsync;
        processor.ProcessErrorAsync += OnErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation(
            "IF-002 EmailConsumer started — Topic={Topic} Subscription={Subscription}",
            _sbOptions.TopicName, _sbOptions.SubscriptionName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // host is stopping
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
            _logger.LogInformation("IF-002 EmailConsumer stopped.");
        }
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        await ConsumeAsync(
            args.Message.Body.ToString(),
            args.Message.MessageId,
            ct => args.CompleteMessageAsync(args.Message, ct),
            ct => args.AbandonMessageAsync(args.Message, cancellationToken: ct),
            args.CancellationToken);
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "IF-002 ServiceBusProcessor error. Source={ErrorSource} EntityPath={EntityPath}",
            args.ErrorSource, args.EntityPath);
        return Task.CompletedTask;
    }

    // ── IEmailConsumer ─────────────────────────────────────────────────────────

    public async Task ConsumeAsync(
        string messageBody,
        string messageId,
        Func<CancellationToken, Task> completeAsync,
        Func<CancellationToken, Task> abandonAsync,
        CancellationToken ct = default)
    {
        CloudEventDto? envelope;
        LeaveNotificationData? data;

        try
        {
            envelope = JsonSerializer.Deserialize<CloudEventDto>(messageBody, DeserializeOptions);
            data = envelope?.Data is JsonElement elem
                ? elem.Deserialize<LeaveNotificationData>(DeserializeOptions)
                : null;

            if (envelope is null || data is null)
                throw new InvalidOperationException("Malformed CloudEvent message — missing envelope or data.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "IF-002 Deserialization failed. MessageId={MessageId} — abandoning (DLQ candidate).",
                messageId);
            await abandonAsync(ct);
            return;
        }

        // ── Idempotency check ─────────────────────────────────────────────────
        var existingLog = await _notificationLogRepo.GetByIdAsync(data.NotificationLogId, ct);
        if (existingLog?.Status == DeliveryStatus.Success)
        {
            _logger.LogInformation(
                "IF-002 Idempotency hit — NotificationLogId={NotificationLogId} already delivered. ACK.",
                data.NotificationLogId);
            await completeAsync(ct);
            return;
        }

        // ── Send email to each recipient ──────────────────────────────────────
        var retryCount = existingLog?.RetryCount ?? 0;

        try
        {
            foreach (var recipient in data.Recipients)
            {
                // ห้าม log recipient email — PII
                var subject = BuildSubject(data.EventType);
                var body = BuildHtmlBody(data);

                await SendEmailAsync(recipient.Email, subject, body, ct);
            }

            await _notificationLogRepo.UpdateDeliveryStatusAsync(
                data.NotificationLogId,
                DeliveryStatus.Success,
                retryCount,
                sentAt: DateTime.UtcNow,
                failureReason: null,
                ct);

            _logger.LogInformation(
                "IF-002 Email delivered. EventType={EventType} NotificationLogId={NotificationLogId} CorrelationId={CorrelationId}",
                data.EventType, data.NotificationLogId, envelope.CorrelationId);

            await completeAsync(ct);
        }
        catch (Exception ex)
        {
            retryCount++;
            _logger.LogWarning(
                ex,
                "IF-002 Email send failed (attempt {Attempt}). EventType={EventType} NotificationLogId={NotificationLogId} — abandoning for SB retry.",
                retryCount, data.EventType, data.NotificationLogId);

            await _notificationLogRepo.UpdateDeliveryStatusAsync(
                data.NotificationLogId,
                DeliveryStatus.Failed,
                retryCount,
                sentAt: null,
                failureReason: ex.Message,
                ct);

            // Never throw — SB retries via MaxDeliveryCount, eventually moves to DLQ
            await abandonAsync(ct);
        }
    }

    public virtual async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = _smtpOptions.EnableSsl, // TLS — enforced at OS/runtime level (TLS 1.2+)
            Credentials = new NetworkCredential(_smtpOptions.UserName, _smtpOptions.Password),
            Timeout = _smtpOptions.TimeoutMs,
        };

        using var message = new MailMessage(
            from: new MailAddress(_smtpOptions.FromAddress, _smtpOptions.FromDisplayName),
            to: new MailAddress(recipientEmail))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        await client.SendMailAsync(message, ct);
    }

    // ── Private email builders ─────────────────────────────────────────────────

    private static string BuildSubject(string eventType) => eventType switch
    {
        "LeaveSubmitted"      => "คำขอลาใหม่รอการอนุมัติ",
        "LeaveApproved"       => "คำขอลาของคุณได้รับการอนุมัติ",
        "LeaveRejected"       => "คำขอลาของคุณถูกปฏิเสธ",
        "CancelRequested"     => "คำขอยกเลิกการลารอการอนุมัติ",
        "CancellationApproved" => "คำขอยกเลิกการลาได้รับการอนุมัติ",
        "CancellationRejected" => "คำขอยกเลิกการลาถูกปฏิเสธ",
        "SlaReminder"         => "แจ้งเตือน: คำขอยกเลิกการลารอการอนุมัติ (เหลือ 4 ชั่วโมง)",
        "SlaEscalated"        => "แจ้งเตือน: คำขอยกเลิกการลาเกินกำหนด SLA",
        _                     => $"แจ้งเตือนระบบลา: {eventType}",
    };

    private static string BuildHtmlBody(LeaveNotificationData data)
    {
        var lines = new List<string>
        {
            $"<p>EventType: <strong>{data.EventType}</strong></p>",
            $"<p>EmployeeId: {data.EmployeeId}</p>",
        };

        if (data.StartDate.HasValue)
            lines.Add($"<p>ช่วงลา: {data.StartDate:yyyy-MM-dd} ถึง {data.EndDate:yyyy-MM-dd} ({data.DurationDays} วัน)</p>");

        if (!string.IsNullOrWhiteSpace(data.RejectionReason))
            lines.Add($"<p>เหตุผล: {data.RejectionReason}</p>");

        return string.Join("\n", lines);
    }
}
