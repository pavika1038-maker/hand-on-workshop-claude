namespace LeaveRequest.Infrastructure.Repositories;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class AttachmentRepository(AppDbContext context) : IAttachmentRepository
{
    public async Task<Attachment?> GetByIdAsync(Guid attachmentId, CancellationToken ct = default)
        => await context.Attachments.FirstOrDefaultAsync(a => a.AttachmentId == attachmentId, ct);

    public async Task<IReadOnlyList<Attachment>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        return await context.Attachments.Where(a => idList.Contains(a.AttachmentId)).ToListAsync(ct);
    }

    // ไม่ดึง Content (byte[]) เพื่อไม่ให้ payload ใหญ่ — detail view ใช้แค่ metadata
    public async Task<IReadOnlyList<Attachment>> GetByLeaveRequestAsync(Guid leaveRequestId, CancellationToken ct = default)
        => await context.Attachments
            .Where(a => a.LeaveRequestId == leaveRequestId)
            .Select(a => new Attachment
            {
                AttachmentId   = a.AttachmentId,
                LeaveRequestId = a.LeaveRequestId,
                FileName       = a.FileName,
                ContentType    = a.ContentType,
                FileSize       = a.FileSize,
                UploadedBy     = a.UploadedBy,
                CreatedAt      = a.CreatedAt,
            })
            .ToListAsync(ct);

    public async Task AddAsync(Attachment attachment, CancellationToken ct = default)
        => await context.Attachments.AddAsync(attachment, ct);

    public void Update(Attachment attachment)
        => context.Attachments.Update(attachment);
}
