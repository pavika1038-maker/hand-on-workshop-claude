namespace LeaveRequest.Domain.Entities;

// IF-004 / SIR-005: ใบรับรองแพทย์แนบกับคำขอลา (VR-007 / BR-006)
public class Attachment
{
    public Guid AttachmentId { get; set; }

    // null จนกว่าจะถูก link ตอน submit คำขอลา (upload มาก่อน submit)
    public Guid? LeaveRequestId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // ponytail: เก็บไฟล์เป็น BLOB ใน SQLite ให้ prototype self-contained
    //           production เปลี่ยนเป็น Azure Blob (StoragePath) ตาม IF-004/SIR-005
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string UploadedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public LeaveRequest? LeaveRequest { get; set; }
}
