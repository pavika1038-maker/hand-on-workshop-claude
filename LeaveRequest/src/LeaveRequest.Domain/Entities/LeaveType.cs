namespace LeaveRequest.Domain.Entities;

public class LeaveType
{
    public byte LeaveTypeId { get; set; }

    // --- Identity ---
    public string TypeCode { get; set; } = string.Empty;      // e.g. "ANNUAL", "SICK"
    public string TypeNameTh { get; set; } = string.Empty;
    public string TypeNameEn { get; set; } = string.Empty;

    // --- Config ---
    public decimal? MaxDaysPerYear { get; set; }               // null = unlimited
    public bool IsAvailableForOutsource { get; set; }
    public bool RequiresMedicalCert { get; set; }

    // --- Audit Columns (Mutable) ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // --- Navigation Properties ---
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
