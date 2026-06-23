namespace LeaveRequest.Application.DTOs;

using LeaveRequest.Domain.Enums;

public record LeaveHistoryFilterRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? EmployeeId { get; init; }
    public byte? LeaveTypeId { get; init; }
    public LeaveStatus? Status { get; init; }
    public string? Department { get; init; }
    public EmployeeType? EmployeeType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record LeaveHistoryItemDto(
    Guid LeaveRequestId,
    string LeaveRequestRef,
    string EmployeeId,
    string EmployeeFullNameTh,
    string EmployeeFullNameEn,
    string? Department,
    EmployeeType EmployeeType,
    byte LeaveTypeId,
    string LeaveTypeNameTh,
    string LeaveTypeNameEn,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DurationDays,
    bool IsHalfDay,
    string? HalfDayPeriod,
    LeaveStatus Status,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? RejectedBy,
    DateTime? RejectedAt,
    string? RejectionReason,
    DateTime CreatedAt
);
