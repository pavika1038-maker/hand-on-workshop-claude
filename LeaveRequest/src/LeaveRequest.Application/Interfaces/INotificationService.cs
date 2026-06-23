namespace LeaveRequest.Application.Interfaces;

public interface INotificationService
{
    // SFR-003, SFR-013 — type: com.abccompany.leave.request.submitted
    Task PublishLeaveSubmittedAsync(Guid leaveRequestId, CancellationToken ct = default);

    // SFR-005 — type: com.abccompany.leave.request.approved
    Task PublishLeaveApprovedAsync(Guid leaveRequestId, CancellationToken ct = default);

    // SFR-005 — type: com.abccompany.leave.request.rejected
    Task PublishLeaveRejectedAsync(Guid leaveRequestId, string rejectionReason, CancellationToken ct = default);

    // SFR-008 — type: com.abccompany.leave.cancel.requested
    Task PublishCancelRequestedAsync(Guid cancelRequestId, CancellationToken ct = default);

    // SFR-009 — type: com.abccompany.leave.cancel.approved
    Task PublishCancellationApprovedAsync(Guid cancelRequestId, CancellationToken ct = default);

    // SFR-009 — type: com.abccompany.leave.cancel.rejected
    Task PublishCancellationRejectedAsync(Guid cancelRequestId, string rejectionReason, CancellationToken ct = default);

    // SFR-010, IF-005 — type: com.abccompany.leave.sla.reminder
    Task PublishSlaReminderAsync(Guid cancelRequestId, CancellationToken ct = default);

    // SFR-010, IF-005 — type: com.abccompany.leave.sla.escalated
    Task PublishSlaEscalatedAsync(Guid cancelRequestId, CancellationToken ct = default);
}
