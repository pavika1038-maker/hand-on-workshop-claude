namespace LeaveRequest.Application.DTOs;

public record ImportResultDto(
    Guid ImportLogId,
    int TotalRecords,
    int SuccessRecords,
    int FailedRecords,
    bool IsRolledBack,
    List<ImportErrorDto> Errors
);

public record ImportErrorDto(
    int RowNumber,
    string Field,
    string Message
);

public record ImportLogSummaryDto(
    Guid ImportLogId,
    string FileName,
    string ImportedBy,
    int TotalRecords,
    int SuccessRecords,
    int FailedRecords,
    bool IsRolledBack,
    DateTime CreatedAt
);
