namespace LeaveRequest.Domain.Entities;

using global::LeaveRequest.Domain.Enums;

public class LeaveRequest
{
    public Guid LeaveRequestId { get; set; }
    public string LeaveRequestRef { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public byte LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DurationDays { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayPeriod { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    // --- Audit Columns ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // --- Navigation Properties ---
    public LeaveType LeaveType { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
