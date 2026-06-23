namespace LeaveRequest.Application.Interfaces;

public interface ISlaSchedulerService
{
    // Send Reminder when SlaDeadline - checkTime ≤ ReminderWindowHours AND SlaReminderSentAt IS NULL
    // Escalate when SlaDeadline ≤ checkTime AND SlaEscalatedAt IS NULL
    Task ProcessSlaEventsAsync(DateTime checkTime, CancellationToken ct = default);
}
