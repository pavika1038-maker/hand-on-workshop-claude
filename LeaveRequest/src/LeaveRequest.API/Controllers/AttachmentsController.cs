namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

// IF-004 / SIR-005: upload + download ใบรับรองแพทย์ (SCR-003 submit, SCR-005 view)
[ApiController]
[Route("api/v1/attachments")]
public class AttachmentsController(IAttachmentRepository attachmentRepo, IUnitOfWork unitOfWork) : ControllerBase
{
    // ponytail: max 5MB — SRS ยังไม่ยืนยัน max size (IF-004 open issue); ปรับได้เมื่อ HR/IT ยืนยัน
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "application/pdf", "image/jpeg", "image/png" };
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    private string CallerEmployeeId =>
        Request.Headers["X-Employee-Id"].FirstOrDefault() ?? string.Empty;

    /// <summary>Upload ไฟล์แนบ (คืน attachmentId เพื่อส่งใน AttachmentIds ตอน submit)</summary>
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("ATTACH_NO_FILE", "กรุณาเลือกไฟล์ก่อน upload"));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse<object>.Fail("ATTACH_FILE_TOO_LARGE",
                $"ไฟล์มีขนาด {file.Length / (1024 * 1024.0):F1} MB เกิน 5 MB ที่กำหนด"));

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse<object>.Fail("ATTACH_WRONG_FILE_TYPE",
                "รองรับเฉพาะไฟล์ PDF, JPG, หรือ PNG เท่านั้น"));

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var attachment = new Attachment
        {
            AttachmentId = Guid.NewGuid(),
            FileName     = file.FileName,
            ContentType  = file.ContentType,
            FileSize     = file.Length,
            Content      = ms.ToArray(),
            UploadedBy   = CallerEmployeeId.Length > 0 ? CallerEmployeeId : "SYSTEM",
            CreatedAt    = DateTime.UtcNow,
        };
        await attachmentRepo.AddAsync(attachment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(ApiResponse<AttachmentUploadResultDto>.Ok(
            new AttachmentUploadResultDto(attachment.AttachmentId, attachment.FileName, attachment.FileSize)));
    }

    /// <summary>ดาวน์โหลด/เปิดดูไฟล์แนบ (SCR-005 — Manager/HR ตรวจสอบ)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var attachment = await attachmentRepo.GetByIdAsync(id, ct);
        if (attachment is null)
            return NotFound(ApiResponse<object>.Fail("ATTACH_NOT_FOUND", "ไม่พบไฟล์แนบที่ระบุ"));

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }
}
