namespace LeaveRequest.Application.Services;

using System.Text.Json;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Entities = LeaveRequest.Domain.Entities;

public sealed class NotificationOptions
{
    public const string SectionName = "Notification";
    public string HrEmail { get; set; } = string.Empty;
    public string SystemSource { get; set; } = "/leave-service/api/v1";
}

public sealed class NotificationService : INotificationService
{
    private readonly ILeaveRequestRepository _leaveRequestRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly INotificationLogRepository _notificationLogRepo;
    private readonly IMessagePublisher _publisher;
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ILeaveRequestRepository leaveRequestRepo,
        IEmployeeRepository employeeRepo,
        INotificationLogRepository notificationLogRepo,
        IMessagePublisher publisher,
        IOptions<NotificationOptions> options,
        ILogger<NotificationService> logger)
    {
        _leaveRequestRepo = leaveRequestRepo;
        _employeeRepo = employeeRepo;
        _notificationLogRepo = notificationLogRepo;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    // ── Public Publish methods ─────────────────────────────────────────────────

    public async Task PublishLeaveSubmittedAsync(Guid leaveRequestId, CancellationToken ct = default)
    {
        var lr = await RequireLeaveRequestAsync(leaveRequestId, ct);
        var emp = await RequireEmployeeAsync(lr.EmployeeId, ct);

        var recipients = new List<NotificationRecipientDto>();
        if (emp.ManagerId is not null)
        {
            var mgr = await _employeeRepo.GetByIdAsync(emp.ManagerId, ct);
            if (mgr is not null) recipients.Add(new(mgr.Email, "Manager"));
        }
        if (!string.IsNullOrWhiteSpace(_options.HrEmail))
            recipients.Add(new(_options.HrEmail, "HR"));

        await PublishLeaveEventAsync(
            "LeaveSubmitted",
            "com.abccompany.leave.request.submitted",
            lr, emp, recipients, rejectionReason: null, ct);
    }

    public async Task PublishLeaveApprovedAsync(Guid leaveRequestId, CancellationToken ct = default)
    {
        var lr = await RequireLeaveRequestAsync(leaveRequestId, ct);
        var emp = await RequireEmployeeAsync(lr.EmployeeId, ct);

        var recipients = new List<NotificationRecipientDto>
        {
            new(emp.Email, "Employee")
        };
        if (!string.IsNullOrWhiteSpace(_options.HrEmail))
            recipients.Add(new(_options.HrEmail, "HR"));

        await PublishLeaveEventAsync(
            "LeaveApproved",
            "com.abccompany.leave.request.approved",
            lr, emp, recipients, rejectionReason: null, ct);
    }

    public async Task PublishLeaveRejectedAsync(Guid leaveRequestId, string rejectionReason, CancellationToken ct = default)
    {
        var lr = await RequireLeaveRequestAsync(leaveRequestId, ct);
        var emp = await RequireEmployeeAsync(lr.EmployeeId, ct);

        var recipients = new List<NotificationRecipientDto>
        {
            new(emp.Email, "Employee")
        };
        if (!string.IsNullOrWhiteSpace(_options.HrEmail))
            recipients.Add(new(_options.HrEmail, "HR"));

        await PublishLeaveEventAsync(
            "LeaveRejected",
            "com.abccompany.leave.request.rejected",
            lr, emp, recipients, rejectionReason, ct);
    }

    public async Task PublishCancelRequestedAsync(Guid cancelRequestId, CancellationToken ct = default)
    {
        // Open Issue OI-003: CancelRequest repository not yet wired — use minimal log
        _logger.LogInformation(
            "IF-002 PublishCancelRequestedAsync called for CancelRequest {CancelRequestId}",
            cancelRequestId);

        await PublishMinimalEventAsync(
            cancelRequestId,
            "CancelRequested",
            "com.abccompany.leave.cancel.requested",
            cancelRequestId: cancelRequestId,
            ct);
    }

    public async Task PublishCancellationApprovedAsync(Guid cancelRequestId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "IF-002 PublishCancellationApprovedAsync called for CancelRequest {CancelRequestId}",
            cancelRequestId);

        await PublishMinimalEventAsync(
            cancelRequestId,
            "CancellationApproved",
            "com.abccompany.leave.cancel.approved",
            cancelRequestId: cancelRequestId,
            ct);
    }

    public async Task PublishCancellationRejectedAsync(Guid cancelRequestId, string rejectionReason, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "IF-002 PublishCancellationRejectedAsync called for CancelRequest {CancelRequestId}",
            cancelRequestId);

        await PublishMinimalEventAsync(
            cancelRequestId,
            "CancellationRejected",
            "com.abccompany.leave.cancel.rejected",
            cancelRequestId: cancelRequestId,
            ct);
    }

    public async Task PublishSlaReminderAsync(Guid cancelRequestId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "IF-002 PublishSlaReminderAsync for CancelRequest {CancelRequestId}",
            cancelRequestId);

        await PublishMinimalEventAsync(
            cancelRequestId,
            "SlaReminder",
            "com.abccompany.leave.sla.reminder",
            cancelRequestId: cancelRequestId,
            ct);
    }

    public async Task PublishSlaEscalatedAsync(Guid cancelRequestId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "IF-002 PublishSlaEscalatedAsync for CancelRequest {CancelRequestId}",
            cancelRequestId);

        await PublishMinimalEventAsync(
            cancelRequestId,
            "SlaEscalated",
            "com.abccompany.leave.sla.escalated",
            cancelRequestId: cancelRequestId,
            ct);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task PublishLeaveEventAsync(
        string eventType,
        string cloudEventType,
        Entities.LeaveRequest lr,
        Employee emp,
        IReadOnlyList<NotificationRecipientDto> recipients,
        string? rejectionReason,
        CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        var notificationLogId = Guid.NewGuid();

        var log = new NotificationLog
        {
            NotificationLogId = notificationLogId,
            EventType = eventType,
            CloudEventType = cloudEventType,
            LeaveRequestId = lr.LeaveRequestId,
            CorrelationId = correlationId,
            RecipientsJson = JsonSerializer.Serialize(recipients),
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
        };
        await _notificationLogRepo.AddAsync(log, ct);

        var data = new LeaveNotificationData(
            NotificationLogId:  notificationLogId,
            EventType:          eventType,
            LeaveRequestId:     lr.LeaveRequestId,
            CancelRequestId:    null,
            EmployeeId:         emp.EmployeeId,
            EmployeeName:       emp.FullNameTh,  // PII — never logged
            LeaveTypeName:      lr.LeaveType?.TypeNameTh,
            StartDate:          lr.StartDate,
            EndDate:            lr.EndDate,
            DurationDays:       lr.DurationDays,
            RejectionReason:    rejectionReason,
            Recipients:         recipients
        );

        var cloudEvent = BuildCloudEvent(cloudEventType, correlationId, notificationLogId, data);

        _logger.LogInformation(
            "IF-002 Publishing {EventType}. NotificationLogId={NotificationLogId} CorrelationId={CorrelationId}",
            eventType, notificationLogId, correlationId);

        await _publisher.PublishAsync(cloudEvent, ct);
    }

    private async Task PublishMinimalEventAsync(
        Guid entityId,
        string eventType,
        string cloudEventType,
        Guid? cancelRequestId,
        CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        var notificationLogId = Guid.NewGuid();

        var log = new NotificationLog
        {
            NotificationLogId = notificationLogId,
            EventType = eventType,
            CloudEventType = cloudEventType,
            CancelRequestId = cancelRequestId,
            CorrelationId = correlationId,
            RecipientsJson = "[]",
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
        };
        await _notificationLogRepo.AddAsync(log, ct);

        var data = new LeaveNotificationData(
            NotificationLogId: notificationLogId,
            EventType: eventType,
            LeaveRequestId: null,
            CancelRequestId: cancelRequestId,
            EmployeeId: string.Empty,
            EmployeeName: string.Empty,
            LeaveTypeName: null,
            StartDate: null,
            EndDate: null,
            DurationDays: null,
            RejectionReason: null,
            Recipients: []
        );

        var cloudEvent = BuildCloudEvent(cloudEventType, correlationId, notificationLogId, data);
        await _publisher.PublishAsync(cloudEvent, default);
    }

    private CloudEventDto BuildCloudEvent(
        string cloudEventType,
        string correlationId,
        Guid notificationLogId,
        object data) => new(
            SpecVersion:     "1.0",
            Type:            cloudEventType,
            Source:          _options.SystemSource,
            Id:              Guid.NewGuid().ToString(),
            Time:            DateTime.UtcNow,
            DataContentType: "application/json",
            CorrelationId:   correlationId,
            Data:            data
        );

    private async Task<Entities.LeaveRequest> RequireLeaveRequestAsync(Guid id, CancellationToken ct)
    {
        var lr = await _leaveRequestRepo.GetByIdAsync(id, ct);
        if (lr is null)
        {
            _logger.LogWarning("IF-002 LeaveRequest {Id} not found — notification skipped.", id);
            throw new InvalidOperationException($"LeaveRequest '{id}' not found for notification.");
        }
        return lr;
    }

    private async Task<Employee> RequireEmployeeAsync(string employeeId, CancellationToken ct)
    {
        var emp = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (emp is null)
        {
            _logger.LogWarning("IF-002 Employee {EmployeeId} not found — notification skipped.", employeeId);
            throw new InvalidOperationException($"Employee '{employeeId}' not found for notification.");
        }
        return emp;
    }
}
