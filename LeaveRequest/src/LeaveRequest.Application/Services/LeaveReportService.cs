namespace LeaveRequest.Application.Services;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;

public class LeaveReportService : ILeaveReportService
{
    private readonly ILeaveReportRepository _repo;

    public LeaveReportService(ILeaveReportRepository repo)
    {
        _repo = repo;
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
}
