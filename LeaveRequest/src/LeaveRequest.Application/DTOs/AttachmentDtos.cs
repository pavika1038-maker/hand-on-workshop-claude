namespace LeaveRequest.Application.DTOs;

// IF-004: ผลการ upload (คืน id เพื่อแนบตอน submit คำขอลา)
public record AttachmentUploadResultDto(
    Guid AttachmentId,
    string FileName,
    long FileSize
);

// metadata ของไฟล์แนบสำหรับแสดงใน SCR-005 (ไม่รวม byte content)
public record AttachmentSummaryDto(
    Guid AttachmentId,
    string FileName,
    string ContentType,
    long FileSize
);
