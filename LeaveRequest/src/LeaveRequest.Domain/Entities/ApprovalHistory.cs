namespace LeaveRequest.Domain.Entities;

using global::LeaveRequest.Domain.Enums;

public class ApprovalHistory
{
    public Guid ApprovalHistoryId { get; set; }
    public Guid? LeaveRequestId   { get; set; }
    public Guid? CancelRequestId  { get; set; }
    public string ApproverId      { get; set; } = string.Empty;
    public ApprovalAction Action  { get; set; }
    public string? Reason         { get; set; }
    public DateTime ActionAt      { get; set; }
    public DateTime CreatedAt     { get; set; }
    public string CreatedBy       { get; set; } = string.Empty;

    // Navigation
    public LeaveRequest? LeaveRequest   { get; set; }
    public CancelRequest? CancelRequest { get; set; }
}
