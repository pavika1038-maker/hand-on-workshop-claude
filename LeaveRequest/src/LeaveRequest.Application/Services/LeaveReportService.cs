namespace LeaveRequest.Application.Services;

using System.Text.Json;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;

public class LeaveReportService : ILeaveReportService
{
    private readonly ILeaveReportRepository _repo;
    private readonly INotificationLogRepository _notifRepo;
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly ICancelRequestRepository _cancelRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public LeaveReportService(
        ILeaveReportRepository repo,
        INotificationLogRepository notifRepo,
        ILeaveRequestRepository leaveRepo,
        ICancelRequestRepository cancelRepo,
        IEmployeeRepository employeeRepo)
    {
        _repo = repo;
        _notifRepo = notifRepo;
        _leaveRepo = leaveRepo;
        _cancelRepo = cancelRepo;
        _employeeRepo = employeeRepo;
    }

    public async Task<PagedResult<LeaveHistoryItemDto>> GetLeaveHistoryAsync(
        LeaveHistoryFilterRequest filter,
        CancellationToken ct = default)
    {
        var query = new LeaveHistoryQuery(
            StartDate: filter.StartDate,
            EndDate: filter.EndDate,
            EmployeeId: filter.EmployeeId,
            LeaveTypeId: filter.LeaveTypeId,
            Status: filter.Status,
            Department: filter.Department,
            EmployeeType: filter.EmployeeType
        );

        var (items, total) = await _repo.GetLeaveHistoryAsync(query, filter.Page, filter.PageSize, ct);

        var dtos = items.Select(lr => new LeaveHistoryItemDto(
            LeaveRequestId: lr.LeaveRequestId,
            LeaveRequestRef: lr.LeaveRequestRef,
            EmployeeId: lr.EmployeeId,
            EmployeeFullNameTh: lr.Employee.FullNameTh,
            EmployeeFullNameEn: lr.Employee.FullNameEn,
            Department: lr.Employee.Department,
            EmployeeType: lr.Employee.EmployeeType,
            LeaveTypeId: lr.LeaveTypeId,
            LeaveTypeNameTh: lr.LeaveType.TypeNameTh,
            LeaveTypeNameEn: lr.LeaveType.TypeNameEn,
            StartDate: lr.StartDate,
            EndDate: lr.EndDate,
            DurationDays: lr.DurationDays,
            IsHalfDay: lr.IsHalfDay,
            HalfDayPeriod: lr.HalfDayPeriod,
            Status: lr.Status,
            ApprovedBy: lr.ApprovedBy,
            ApprovedAt: lr.ApprovedAt,
            RejectedBy: lr.RejectedBy,
            RejectedAt: lr.RejectedAt,
            RejectionReason: lr.RejectionReason,
            CreatedAt: lr.CreatedAt
        )).ToList();

        return new PagedResult<LeaveHistoryItemDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // SF-015 / RP-003: Notification Log View
    public async Task<NotificationLogReportDto> GetNotificationLogAsync(
        NotificationLogFilterRequest filter,
        CancellationToken ct = default)
    {
        var (logs, total, success, failed) = await _notifRepo.GetForReportAsync(
            filter.DateFrom, filter.DateTo, filter.EventType, filter.Recipient, filter.Status,
            filter.Page, filter.PageSize, ct);

        var items = new List<NotificationLogItemDto>(logs.Count);
        foreach (var log in logs)
        {
            string? requestRef = null;
            string? employeeName = null;

            // ponytail: resolve ref/ชื่อ ทีละ log — หน้าละ ≤ pageSize แถว (ยอมรับได้สำหรับ prototype)
            if (log.LeaveRequestId is { } lrId)
            {
                var lr = await _leaveRepo.GetByIdAsync(lrId, ct);
                requestRef = lr?.LeaveRequestRef;
                employeeName = lr?.Employee?.FullNameTh;
            }
            else if (log.CancelRequestId is { } crId)
            {
                var cr = await _cancelRepo.GetByIdAsync(crId, ct);
                requestRef = cr?.CancelRequestRef;
                if (cr is not null)
                    employeeName = (await _employeeRepo.GetByIdAsync(cr.EmployeeId, ct))?.FullNameTh;
            }

            items.Add(new NotificationLogItemDto(
                NotificationLogId: log.NotificationLogId,
                SentAt: log.SentAt,
                CreatedAt: log.CreatedAt,
                EventType: log.EventType,
                RequestRef: requestRef,
                LeaveRequestId: log.LeaveRequestId,
                EmployeeName: employeeName,
                Recipients: ParseRecipients(log.RecipientsJson),
                Status: log.Status.ToString(),
                RetryCount: log.RetryCount,
                FailureReason: log.FailureReason
            ));
        }

        var rate = total > 0 ? Math.Round((decimal)success / total * 100, 1) : 0m;

        return new NotificationLogReportDto(
            TotalCount: total,
            SuccessCount: success,
            FailedCount: failed,
            SuccessRatePct: rate,
            Items: new PagedResult<NotificationLogItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            }
        );
    }

    // SF-014 / RP-001: Leave Summary Report — aggregate ใน memory (ponytail: prototype dataset เล็ก)
    public async Task<LeaveSummaryReportDto> GetLeaveSummaryAsync(
        LeaveHistoryFilterRequest filter,
        CancellationToken ct = default)
    {
        var query = new LeaveHistoryQuery(
            StartDate: filter.StartDate,
            EndDate: filter.EndDate,
            LeaveTypeId: filter.LeaveTypeId,
            Department: filter.Department,
            EmployeeType: filter.EmployeeType
        );

        var all = await _repo.GetForSummaryAsync(query, ct);

        static decimal Rate(int part, int whole) => whole > 0 ? Math.Round((decimal)part / whole * 100, 1) : 0m;

        var rows = all
            .GroupBy(r => new { Dept = r.Employee.Department ?? "(ไม่ระบุแผนก)", r.LeaveTypeId, r.LeaveType.TypeNameTh })
            .Select(g =>
            {
                var count    = g.Count();
                var approved = g.Count(x => x.Status == LeaveStatus.Approved);
                return new LeaveSummaryRowDto(
                    Department: g.Key.Dept,
                    LeaveTypeId: g.Key.LeaveTypeId,
                    LeaveTypeName: g.Key.TypeNameTh,
                    RequestCount: count,
                    TotalLeaveDays: g.Sum(x => x.DurationDays),
                    ApprovedCount: approved,
                    RejectedCount: g.Count(x => x.Status == LeaveStatus.Rejected),
                    CancelledCount: g.Count(x => x.Status == LeaveStatus.Cancelled),
                    ApproveRatePct: Rate(approved, count));
            })
            .OrderBy(r => r.Department)
            .ThenByDescending(r => r.TotalLeaveDays)
            .ThenBy(r => r.LeaveTypeId)
            .ToList();

        var byType = all
            .GroupBy(r => r.Employee.EmployeeType)
            .Select(g =>
            {
                var count = g.Count();
                return new LeaveSummaryGroupDto(
                    EmployeeType: g.Key == Domain.Enums.EmployeeType.Outsource ? "Outsource" : "ประจำ",
                    RequestCount: count,
                    TotalLeaveDays: g.Sum(x => x.DurationDays),
                    ApproveRatePct: Rate(g.Count(x => x.Status == LeaveStatus.Approved), count),
                    RejectRatePct: Rate(g.Count(x => x.Status == LeaveStatus.Rejected), count));
            })
            .OrderBy(g => g.EmployeeType)
            .ToList();

        var grandTotal = all.Count;
        return new LeaveSummaryReportDto(
            Rows: rows,
            ByEmployeeType: byType,
            GrandTotalRequests: grandTotal,
            GrandTotalDays: all.Sum(x => x.DurationDays),
            ApproveRatePct: Rate(all.Count(x => x.Status == LeaveStatus.Approved), grandTotal),
            RejectRatePct: Rate(all.Count(x => x.Status == LeaveStatus.Rejected), grandTotal),
            DistinctEmployees: all.Select(x => x.EmployeeId).Distinct().Count()
        );
    }

    private static string ParseRecipients(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return "";
        try
        {
            var arr = JsonSerializer.Deserialize<List<string>>(json);
            return arr is not null ? string.Join(", ", arr) : json;
        }
        catch
        {
            return json;
        }
    }
}
