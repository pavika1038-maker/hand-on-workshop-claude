namespace LeaveRequest.Application.Interfaces;

public interface IEmailConsumer
{
    // Called by the hosted consumer for each Service Bus message delivered to "email-notify".
    // Idempotency: checks NotificationLog status before processing.
    // DLQ: abandons message on failure (never throws — lets SB retry).
    Task ConsumeAsync(
        string messageBody,
        string messageId,
        Func<CancellationToken, Task> completeAsync,
        Func<CancellationToken, Task> abandonAsync,
        CancellationToken ct = default);

    // Send email via SMTP (TLS 1.2+).
    // Used internally by ConsumeAsync; exposed for testing.
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default);
}
