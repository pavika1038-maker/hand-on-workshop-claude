namespace LeaveRequest.Infrastructure.SlaScheduler;

public sealed class SlaSchedulerOptions
{
    public const string SectionName = "SlaScheduler";

    // Spec: 5 minutes (NFR-011: delay tolerance ≤ 15 min)
    public int IntervalMinutes { get; set; } = 5;

    // BR-018: send Reminder when SlaDeadline - now ≤ 4 hours
    public double ReminderWindowHours { get; set; } = 4;
}
