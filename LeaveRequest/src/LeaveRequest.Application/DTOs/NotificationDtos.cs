namespace LeaveRequest.Application.DTOs;

// CloudEvents 1.0 envelope — architecture §10.1
public record CloudEventDto(
    string SpecVersion,
    string Type,
    string Source,
    string Id,
    DateTime Time,
    string DataContentType,
    string CorrelationId,
    object Data             // serialised as JSON object in the message body
);

// Recipient entry embedded in CloudEvent data
public record NotificationRecipientDto(string Email, string Role);

// Inner data payload for all leave-event CloudEvents
public record LeaveNotificationData(
    Guid NotificationLogId,
    string EventType,
    Guid? LeaveRequestId,
    Guid? CancelRequestId,
    string EmployeeId,
    string EmployeeName,       // FullNameTh — never logged (PII)
    string? LeaveTypeName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? DurationDays,
    string? RejectionReason,
    IReadOnlyList<NotificationRecipientDto> Recipients
);
