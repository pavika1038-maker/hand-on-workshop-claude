namespace LeaveRequest.Domain.Entities;

using global::LeaveRequest.Domain.Enums;

public class Employee
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameTh { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public string? ManagerId { get; set; }
    public EmployeeType EmployeeType { get; set; } = EmployeeType.Regular;
    public string? AgencyCompany { get; set; }
    public DateOnly? AbcStartDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncedAt { get; set; }

    // --- Audit Columns ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // --- Navigation Properties ---
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
