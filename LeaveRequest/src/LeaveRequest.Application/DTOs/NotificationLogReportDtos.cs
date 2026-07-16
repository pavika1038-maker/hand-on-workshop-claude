namespace LeaveRequest.Application.DTOs;

using LeaveRequest.Domain.Enums;

// SF-015 / RP-003: filter สำหรับ Notification Log View
public record NotificationLogFilterRequest
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? EventType { get; init; }
    public string? Recipient { get; init; }
    public DeliveryStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record NotificationLogItemDto(
    Guid NotificationLogId,
    DateTime? SentAt,
    DateTime CreatedAt,
    string EventType,
    string? RequestRef,       // LeaveRequestRef หรือ CancelRequestRef
    Guid? LeaveRequestId,     // สำหรับ link ไป detail (null ถ้าเป็น cancel-only)
    string? EmployeeName,
    string Recipients,        // join จาก RecipientsJson
    string Status,
    int RetryCount,
    string? FailureReason
);

// ห่อ summary + หน้าปัจจุบัน (summary คำนวณจากทั้ง filter ไม่ใช่แค่หน้าเดียว)
public record NotificationLogReportDto(
    int TotalCount,
    int SuccessCount,
    int FailedCount,
    decimal SuccessRatePct,
    PagedResult<NotificationLogItemDto> Items
);
