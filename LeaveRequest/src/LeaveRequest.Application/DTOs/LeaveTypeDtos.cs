namespace LeaveRequest.Application.DTOs;

public record LeaveTypeListItemDto(
    byte LeaveTypeId,
    string TypeCode,
    string TypeNameTh,
    string TypeNameEn,
    decimal? MaxDaysPerYear,
    bool IsAvailableForOutsource,
    bool RequiresMedicalCert
);

public record LeaveTypeDetailDto(
    byte LeaveTypeId,
    string TypeCode,
    string TypeNameTh,
    string TypeNameEn,
    decimal? MaxDaysPerYear,
    bool IsAvailableForOutsource,
    bool RequiresMedicalCert,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

public record CreateLeaveTypeRequest(
    string TypeCode,
    string TypeNameTh,
    string TypeNameEn,
    decimal? MaxDaysPerYear,
    bool IsAvailableForOutsource,
    bool RequiresMedicalCert
);

public record UpdateLeaveTypeRequest(
    string TypeCode,
    string TypeNameTh,
    string TypeNameEn,
    decimal? MaxDaysPerYear,
    bool IsAvailableForOutsource,
    bool RequiresMedicalCert
);
