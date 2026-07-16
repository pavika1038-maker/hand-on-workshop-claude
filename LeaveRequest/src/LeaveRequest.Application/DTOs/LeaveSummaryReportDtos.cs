namespace LeaveRequest.Application.DTOs;

// RP-001 (SF-014): รายงานสรุปการลา — group by แผนก × ประเภทการลา

public record LeaveSummaryRowDto(
    string Department,
    byte LeaveTypeId,
    string LeaveTypeName,
    int RequestCount,
    decimal TotalLeaveDays,
    int ApprovedCount,
    int RejectedCount,
    int CancelledCount,
    decimal ApproveRatePct
);

// Summary strip แยกตามประเภทพนักงาน (ประจำ / Outsource)
public record LeaveSummaryGroupDto(
    string EmployeeType,
    int RequestCount,
    decimal TotalLeaveDays,
    decimal ApproveRatePct,
    decimal RejectRatePct
);

public record LeaveSummaryReportDto(
    IReadOnlyList<LeaveSummaryRowDto> Rows,
    IReadOnlyList<LeaveSummaryGroupDto> ByEmployeeType,
    int GrandTotalRequests,
    decimal GrandTotalDays,
    decimal ApproveRatePct,
    decimal RejectRatePct,
    int DistinctEmployees
);
