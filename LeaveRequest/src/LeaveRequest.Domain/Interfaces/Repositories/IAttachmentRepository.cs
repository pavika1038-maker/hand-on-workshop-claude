namespace LeaveRequest.Domain.Interfaces.Repositories;

using LeaveRequest.Domain.Entities;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(Guid attachmentId, CancellationToken ct = default);

    /// <summary>ดึง attachment ตาม id หลายรายการ (ใช้ตอน link เข้าคำขอลา)</summary>
    Task<IReadOnlyList<Attachment>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>ดึง metadata ของ attachment ที่ผูกกับคำขอลา (SCR-005 detail — ไม่รวม byte content)</summary>
    Task<IReadOnlyList<Attachment>> GetByLeaveRequestAsync(Guid leaveRequestId, CancellationToken ct = default);

    Task AddAsync(Attachment attachment, CancellationToken ct = default);
    void Update(Attachment attachment);
}
