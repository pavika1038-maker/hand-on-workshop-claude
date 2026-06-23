namespace LeaveRequest.Domain.Entities;

using global::LeaveRequest.Domain.Enums;

public class NotificationLog
{
    public Guid NotificationLogId { get; set; }
    public string EventType { get; set; } = string.Empty;     // e.g. "LeaveSubmitted"
    public string CloudEventType { get; set; } = string.Empty; // e.g. "com.abccompany.leave.request.submitted"
    public Guid? LeaveRequestId { get; set; }
    public Guid? CancelRequestId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string RecipientsJson { get; set; } = string.Empty; // JSON array of recipient emails
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }

    // --- Audit Columns ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
