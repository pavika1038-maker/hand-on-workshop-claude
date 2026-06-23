namespace LeaveRequest.Domain.Enums;

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    CancelRequested = 5,
    Escalated = 6
}
