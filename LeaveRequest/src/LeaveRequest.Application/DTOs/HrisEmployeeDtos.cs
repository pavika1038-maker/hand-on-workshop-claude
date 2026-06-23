namespace LeaveRequest.Application.DTOs;

public record HrisEmployeeDto(
    string EmployeeId,
    string EmployeeCode,
    string FullNameTh,
    string FullNameEn,
    string? Department,
    string? Position,
    string Email,
    DateOnly HireDate,
    string? ManagerId,
    bool IsActive
);
