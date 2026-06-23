namespace LeaveRequest.Domain.Entities;

public class ImportLog
{
    public Guid ImportLogId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ImportedBy { get; set; } = string.Empty;    // HR employee ID
    public int TotalRecords { get; set; }
    public int SuccessRecords { get; set; }
    public int FailedRecords { get; set; }
    public string ErrorDetailsJson { get; set; } = "[]";       // JSON: ImportErrorDto[]
    public bool IsRolledBack { get; set; }                     // true when >50% rows failed

    // --- Audit Columns (Immutable — no Update) ---
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
