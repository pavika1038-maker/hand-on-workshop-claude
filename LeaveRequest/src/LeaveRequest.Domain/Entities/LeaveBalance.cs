namespace LeaveRequest.Domain.Entities;

public class LeaveBalance
{
    public Guid LeaveBalanceId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public byte LeaveTypeId { get; set; }
    public int LeaveYear { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal CarriedForwardDays { get; set; }

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

    // Calculated: สิทธิ์คงเหลือ = entitled + carry-forward - used - pending
    public decimal RemainingDays => EntitledDays + CarriedForwardDays - UsedDays - PendingDays;
}
