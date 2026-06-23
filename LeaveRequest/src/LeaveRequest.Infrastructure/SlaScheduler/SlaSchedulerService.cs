namespace LeaveRequest.Infrastructure.SlaScheduler;

using System.Diagnostics;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class SlaSchedulerService : BackgroundService, ISlaSchedulerService
{
    private readonly IServiceScopeFactory? _scopeFactory;

    // Set only via internal test constructor
    private readonly ICancelRequestRepository? _cancelRepo;
    private readonly INotificationService? _notificationService;
    private readonly AppDbContext? _context;

    private readonly SlaSchedulerOptions _options;
    private readonly ILogger<SlaSchedulerService> _logger;

    // Production constructor — DI uses IServiceScopeFactory (BackgroundService is singleton)
    public SlaSchedulerService(
        IServiceScopeFactory scopeFactory,
        IOptions<SlaSchedulerOptions> options,
        ILogger<SlaSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    // Test constructor (InternalsVisibleTo → LeaveRequest.Application.Tests)
    internal SlaSchedulerService(
        ICancelRequestRepository cancelRepo,
        INotificationService notificationService,
        AppDbContext context,
        SlaSchedulerOptions options,
        ILogger<SlaSchedulerService> logger)
    {
        _cancelRepo = cancelRepo;
        _notificationService = notificationService;
        _context = context;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "IF-005 SlaSchedulerService started. Interval={Interval}min ReminderWindow={Window}h",
            _options.IntervalMinutes, _options.ReminderWindowHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            var checkTime = DateTime.UtcNow;
            try
            {
                await ProcessSlaEventsAsync(checkTime, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IF-005 Unhandled error in SLA run. CheckTime={CheckTime:O}", checkTime);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.IntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("IF-005 SlaSchedulerService stopped.");
    }

    public async Task ProcessSlaEventsAsync(DateTime checkTime, CancellationToken ct = default)
    {
        ICancelRequestRepository cancelRepo;
        INotificationService notificationService;
        AppDbContext context;
        IServiceScope? scope = null;

        if (_scopeFactory is not null)
        {
            scope = _scopeFactory.CreateScope();
            cancelRepo        = scope.ServiceProvider.GetRequiredService<ICancelRequestRepository>();
            notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            context           = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }
        else
        {
            // Test path: dependencies injected directly
            cancelRepo        = _cancelRepo!;
            notificationService = _notificationService!;
            context           = _context!;
        }

        try
        {
            await RunBatchAsync(checkTime, cancelRepo, notificationService, context, ct);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private async Task RunBatchAsync(
        DateTime checkTime,
        ICancelRequestRepository cancelRepo,
        INotificationService notificationService,
        AppDbContext context,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var processed = 0;
        var reminders = 0;
        var escalated = 0;
        var errors    = 0;

        _logger.LogInformation("IF-005 SLA check started. CheckTime={CheckTime:O}", checkTime);

        var pendingRequests = await cancelRepo.GetPendingForSlaCheckAsync(checkTime, ct);

        foreach (var cr in pendingRequests)
        {
            try
            {
                // Escalate takes priority over Reminder
                bool needsEscalation = cr.SlaDeadline <= checkTime
                    && cr.SlaEscalatedAt is null;

                bool needsReminder = cr.SlaDeadline.AddHours(-_options.ReminderWindowHours) <= checkTime
                    && cr.SlaReminderSentAt is null;

                if (needsEscalation)
                {
                    await notificationService.PublishSlaEscalatedAsync(cr.CancelRequestId, ct);
                    cr.SlaEscalatedAt = checkTime;
                    cr.Status         = CancelRequestStatus.Escalated;
                    cr.UpdatedAt      = checkTime;
                    cr.UpdatedBy      = "system";
                    await cancelRepo.UpdateAsync(cr, ct);
                    await context.SaveChangesAsync(ct);
                    escalated++;
                }
                else if (needsReminder)
                {
                    await notificationService.PublishSlaReminderAsync(cr.CancelRequestId, ct);
                    cr.SlaReminderSentAt = checkTime;
                    cr.UpdatedAt         = checkTime;
                    cr.UpdatedBy         = "system";
                    await cancelRepo.UpdateAsync(cr, ct);
                    await context.SaveChangesAsync(ct);
                    reminders++;
                }

                processed++;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                errors++;
                _logger.LogWarning(ex,
                    "IF-005 Failed to process CancelRequestId={CancelRequestId}. Skipping.",
                    cr.CancelRequestId);
            }
        }

        sw.Stop();
        _logger.LogInformation(
            "IF-005 SLA check completed. CheckTime={CheckTime:O} Processed={Processed} " +
            "Reminders={Reminders} Escalated={Escalated} Errors={Errors} Duration={Duration}ms",
            checkTime, processed, reminders, escalated, errors, sw.ElapsedMilliseconds);
    }
}
