namespace LeaveRequest.Domain.Entities;

using global::LeaveRequest.Domain.Enums;

public class CancelRequest
{
    public Guid CancelRequestId { get; set; }
    public string CancelRequestRef { get; set; } = string.Empty;

    public Guid LeaveRequestId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;

    public string? Reason { get; set; }
    public CancelRequestStatus Status { get; set; } = CancelRequestStatus.Pending;

    // SLA fields — pre-computed when CancelRequest is submitted
    public DateTime SlaDeadline { get; set; }
    public DateTime? SlaReminderSentAt { get; set; }
    public DateTime? SlaEscalatedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public LeaveRequest? LeaveRequest { get; set; }
}
