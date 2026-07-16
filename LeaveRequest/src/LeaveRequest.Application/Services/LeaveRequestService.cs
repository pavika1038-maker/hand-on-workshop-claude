namespace LeaveRequest.Application.Services;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

public class LeaveRequestService(
    ILeaveRequestRepository leaveRequestRepo,
    ILeaveBalanceRepository balanceRepo,
    IEmployeeRepository employeeRepo,
    ILeaveTypeRepository leaveTypeRepo,
    ICancelRequestRepository cancelRequestRepo,
    IApprovalHistoryRepository approvalHistoryRepo,
    IAttachmentRepository attachmentRepo,
    IUnitOfWork unitOfWork,
    ILogger<LeaveRequestService> logger
) : ILeaveRequestService
{
    // ── Submit (SFR-003) ──────────────────────────────────────────────────────

    public async Task<LeaveRequestResult> SubmitLeaveRequestAsync(
        string employeeId, CreateLeaveRequestDto request, CancellationToken ct = default)
    {
        if (request.IsHalfDay && request.HalfDayPeriod is not ("AM" or "PM"))
            throw new BusinessException("HalfDayPeriod ต้องเป็น AM หรือ PM เท่านั้น", "INVALID_HALF_DAY");

        var employee = await employeeRepo.GetByIdAsync(employeeId, ct)
            ?? throw new NotFoundException(nameof(Employee), employeeId);
        if (!employee.IsActive)
            throw new NotFoundException(nameof(Employee), employeeId);

        var leaveType = await leaveTypeRepo.GetByIdAsync(request.LeaveTypeId, ct)
            ?? throw new NotFoundException(nameof(LeaveType), request.LeaveTypeId);

        if (employee.EmployeeType == EmployeeType.Outsource && !leaveType.IsAvailableForOutsource)
            throw new BusinessException($"พนักงาน Outsource ไม่มีสิทธิ์ลา '{leaveType.TypeNameTh}'", "VR-001");

        var durationDays = request.IsHalfDay
            ? 0.5m
            : (decimal)(request.EndDate.DayNumber - request.StartDate.DayNumber + 1);

        // VR-007 / BR-006: ลาที่ต้องใบรับรองแพทย์ (เช่น ลาป่วย) ตั้งแต่ 3 วันขึ้นไป ต้องแนบไฟล์
        // ponytail: ใช้ durationDays (calendar span) แทน working-day count — ยกระดับเป็นวันทำการเมื่อ business ยืนยันนิยาม
        if (leaveType.RequiresMedicalCert && durationDays >= 3 && request.AttachmentIds.Count == 0)
            throw new BusinessException(
                $"การลา '{leaveType.TypeNameTh}' ตั้งแต่ 3 วันขึ้นไป ต้องแนบใบรับรองแพทย์", "VR-007");

        if (leaveType.MaxDaysPerYear.HasValue)
        {
            var balance = await balanceRepo.GetAsync(employeeId, request.LeaveTypeId, request.StartDate.Year, ct);
            var remaining = balance?.RemainingDays ?? 0m;
            if (remaining < durationDays)
                throw new BusinessException(
                    $"สิทธิ์ลา '{leaveType.TypeNameTh}' ไม่เพียงพอ: คงเหลือ {remaining} วัน ต้องการ {durationDays} วัน",
                    "INSUFFICIENT_BALANCE");
        }

        var overlapping = await leaveRequestRepo.GetOverlappingAsync(
            employeeId, request.StartDate, request.EndDate, ct);
        if (overlapping.Any())
            throw new BusinessException(
                $"มีคำขอลาช่วง {request.StartDate:yyyy-MM-dd} – {request.EndDate:yyyy-MM-dd} ซ้อนทับกับคำขออื่น",
                "DATE_CONFLICT");

        var leaveRequestRef = $"LR-{request.StartDate.Year}-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
        var entity = new LeaveRequest
        {
            LeaveRequestId  = Guid.NewGuid(),
            LeaveRequestRef = leaveRequestRef,
            EmployeeId      = employeeId,
            LeaveTypeId     = request.LeaveTypeId,
            StartDate       = request.StartDate,
            EndDate         = request.EndDate,
            DurationDays    = durationDays,
            IsHalfDay       = request.IsHalfDay,
            HalfDayPeriod   = request.HalfDayPeriod,
            Reason          = request.Reason,
            Status          = LeaveStatus.Pending,
            CreatedAt       = DateTime.UtcNow,
            CreatedBy       = employeeId,
        };

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await leaveRequestRepo.AddAsync(entity, ct);

            // IF-004: link ไฟล์แนบ (upload มาก่อนแล้ว) เข้ากับคำขอลานี้
            if (request.AttachmentIds.Count > 0)
            {
                var attachments = await attachmentRepo.GetByIdsAsync(request.AttachmentIds, ct);
                foreach (var att in attachments)
                {
                    att.LeaveRequestId = entity.LeaveRequestId;
                    attachmentRepo.Update(att);
                }
            }

            if (leaveType.MaxDaysPerYear.HasValue)
            {
                var balance = await balanceRepo.GetAsync(employeeId, request.LeaveTypeId, request.StartDate.Year, ct);
                if (balance != null)
                {
                    balance.PendingDays += durationDays;
                    balance.UpdatedAt = DateTime.UtcNow;
                    balance.UpdatedBy = employeeId;
                    balanceRepo.Update(balance);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            logger.LogInformation("Leave {Ref} submitted by {EmpId}", leaveRequestRef, employeeId);
            return new LeaveRequestResult(entity.LeaveRequestId, leaveRequestRef, LeaveStatus.Pending,
                "ยื่นคำขอลาสำเร็จ กรุณารอการอนุมัติจากผู้บังคับบัญชา");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ── My List (SCR-003) ─────────────────────────────────────────────────────

    public async Task<PagedResult<LeaveRequestSummaryDto>> GetMyRequestsAsync(
        string employeeId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await leaveRequestRepo.GetByEmployeeAsync(employeeId, page, pageSize, ct);
        var dtos = items.Select(r => new LeaveRequestSummaryDto(
            r.LeaveRequestId,
            r.LeaveRequestRef,
            r.LeaveType.TypeNameTh,
            r.StartDate,
            r.EndDate,
            r.DurationDays,
            r.IsHalfDay,
            r.Reason,
            r.Status.ToString(),
            r.CreatedAt
        )).ToList();
        return new PagedResult<LeaveRequestSummaryDto>
        { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    // ── Detail (SCR-005) ──────────────────────────────────────────────────────

    public async Task<LeaveRequestDetailDto> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var r = await leaveRequestRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), id);
        var attachments = await attachmentRepo.GetByLeaveRequestAsync(id, ct);
        return new LeaveRequestDetailDto(
            r.LeaveRequestId,
            r.LeaveRequestRef,
            r.EmployeeId,
            r.Employee.FullNameTh,
            r.LeaveType.TypeNameTh,
            r.StartDate,
            r.EndDate,
            r.DurationDays,
            r.IsHalfDay,
            r.HalfDayPeriod,
            r.Reason,
            r.Status.ToString(),
            r.ApprovedBy,
            r.ApprovedAt,
            r.RejectedBy,
            r.RejectedAt,
            r.RejectionReason,
            r.CreatedAt,
            attachments.Select(a => new AttachmentSummaryDto(
                a.AttachmentId, a.FileName, a.ContentType, a.FileSize)).ToList()
        );
    }

    // ── Audit Trail Timeline (SCR-005 / SF-013) ───────────────────────────────

    public async Task<IReadOnlyList<TimelineEventDto>> GetTimelineAsync(
        Guid leaveRequestId, CancellationToken ct = default)
    {
        var lr = await leaveRequestRepo.GetByIdAsync(leaveRequestId, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        var events = new List<TimelineEventDto>();
        var nameCache = new Dictionary<string, string>();

        async Task<string> Name(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "-";
            if (id == "SYSTEM") return "ระบบ";
            if (nameCache.TryGetValue(id, out var cached)) return cached;
            var emp = await employeeRepo.GetByIdAsync(id, ct);
            var name = emp?.FullNameTh ?? id;
            nameCache[id] = name;
            return name;
        }

        // Created
        events.Add(new TimelineEventDto("Created", "สร้างคำขอ", await Name(lr.CreatedBy), lr.CreatedAt, null));

        // Approve / Reject บน LeaveRequest
        foreach (var h in await approvalHistoryRepo.GetByLeaveRequestAsync(leaveRequestId, ct))
        {
            var (type, label) = h.Action == ApprovalAction.Approved
                ? ("Approved", "อนุมัติคำขอ")
                : ("Rejected", "ปฏิเสธคำขอ");
            events.Add(new TimelineEventDto(type, label, await Name(h.ApproverId), h.ActionAt, h.Reason));
        }

        // Cancel Requests + การพิจารณา
        var cancels = await cancelRequestRepo.GetByLeaveRequestAsync(leaveRequestId, ct);
        foreach (var cr in cancels)
            events.Add(new TimelineEventDto("CancelRequested", "ขอยกเลิก", await Name(cr.EmployeeId), cr.CreatedAt, cr.Reason));

        var cancelHist = await approvalHistoryRepo.GetByCancelRequestIdsAsync(
            cancels.Select(c => c.CancelRequestId).ToList(), ct);
        foreach (var h in cancelHist)
        {
            var (type, label) = h.Action == ApprovalAction.Approved
                ? ("CancellationApproved", "อนุมัติการยกเลิก")
                : ("CancellationRejected", "ปฏิเสธการยกเลิก");
            events.Add(new TimelineEventDto(type, label, await Name(h.ApproverId), h.ActionAt, h.Reason));
        }

        // ยกเลิกทันที (Pending → Cancelled โดยไม่มี CancelRequest — SF-007)
        if (lr.Status == LeaveStatus.Cancelled && cancels.Count == 0 && lr.UpdatedAt.HasValue)
            events.Add(new TimelineEventDto("Cancelled", "ยกเลิกคำขอ", await Name(lr.UpdatedBy), lr.UpdatedAt.Value, null));

        return events.OrderBy(e => e.ActionAt).ToList();
    }

    // ── Cancel (SCR-006) ──────────────────────────────────────────────────────

    public async Task<string> CancelAsync(
        Guid leaveRequestId, string employeeId, string? reason, CancellationToken ct = default)
    {
        var lr = await leaveRequestRepo.GetByIdAsync(leaveRequestId, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        if (lr.EmployeeId != employeeId)
            throw new BusinessException("ไม่มีสิทธิ์ยกเลิกคำร้องนี้", "FORBIDDEN");

        if (lr.Status == LeaveStatus.Pending)
        {
            // ยกเลิกทันที
            lr.Status    = LeaveStatus.Cancelled;
            lr.UpdatedAt = DateTime.UtcNow;
            lr.UpdatedBy = employeeId;
            leaveRequestRepo.Update(lr);

            // คืน PendingDays
            var balance = await balanceRepo.GetAsync(
                employeeId, lr.LeaveTypeId, lr.StartDate.Year, ct);
            if (balance != null)
            {
                balance.PendingDays = Math.Max(0, balance.PendingDays - lr.DurationDays);
                balance.UpdatedAt   = DateTime.UtcNow;
                balance.UpdatedBy   = employeeId;
                balanceRepo.Update(balance);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return "ยกเลิกคำร้องสำเร็จ";
        }

        if (lr.Status == LeaveStatus.Approved)
        {
            // สร้าง CancelRequest รอ Manager อนุมัติ
            lr.Status    = LeaveStatus.CancelRequested;
            lr.UpdatedAt = DateTime.UtcNow;
            lr.UpdatedBy = employeeId;
            leaveRequestRepo.Update(lr);

            var cancelRef = $"CR-{DateTime.UtcNow.Year}-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
            var cr = new CancelRequest
            {
                CancelRequestId  = Guid.NewGuid(),
                CancelRequestRef = cancelRef,
                LeaveRequestId   = leaveRequestId,
                EmployeeId       = employeeId,
                Reason           = reason,
                Status           = CancelRequestStatus.Pending,
                SlaDeadline      = DateTime.UtcNow.AddHours(48),
                CreatedAt        = DateTime.UtcNow,
                CreatedBy        = employeeId,
            };
            await cancelRequestRepo.AddAsync(cr, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return "ส่งคำขอยกเลิกแล้ว รอ Manager อนุมัติ";
        }

        throw new BusinessException($"ไม่สามารถยกเลิกคำร้องที่มีสถานะ '{lr.Status}' ได้", "INVALID_STATUS");
    }

    // ── Approve (SCR-004) ─────────────────────────────────────────────────────

    public async Task ApproveAsync(
        Guid leaveRequestId, string managerId, string? comment, CancellationToken ct = default)
    {
        var lr = await leaveRequestRepo.GetByIdAsync(leaveRequestId, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        if (lr.Status != LeaveStatus.Pending)
            throw new BusinessException($"ไม่สามารถอนุมัติคำร้องที่มีสถานะ '{lr.Status}'", "INVALID_STATUS");

        if (string.IsNullOrWhiteSpace(managerId) || lr.Employee.ManagerId != managerId)
            throw new BusinessException("ไม่มีสิทธิ์อนุมัติคำร้องนี้", "FORBIDDEN");

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            lr.Status     = LeaveStatus.Approved;
            lr.ApprovedBy = managerId;
            lr.ApprovedAt = DateTime.UtcNow;
            lr.UpdatedAt  = DateTime.UtcNow;
            lr.UpdatedBy  = managerId;
            leaveRequestRepo.Update(lr);

            // ย้าย PendingDays → UsedDays
            var balance = await balanceRepo.GetAsync(lr.EmployeeId, lr.LeaveTypeId, lr.StartDate.Year, ct);
            if (balance != null)
            {
                balance.PendingDays = Math.Max(0, balance.PendingDays - lr.DurationDays);
                balance.UsedDays   += lr.DurationDays;
                balance.UpdatedAt   = DateTime.UtcNow;
                balance.UpdatedBy   = managerId;
                balanceRepo.Update(balance);
            }

            await approvalHistoryRepo.AddAsync(new ApprovalHistory
            {
                ApprovalHistoryId = Guid.NewGuid(),
                LeaveRequestId    = lr.LeaveRequestId,
                ApproverId        = managerId,
                Action            = ApprovalAction.Approved,
                Reason            = comment,
                ActionAt          = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                CreatedBy         = managerId,
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Leave {Id} approved by {Mgr}", leaveRequestId, managerId);
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    // ── Reject (SCR-004) ──────────────────────────────────────────────────────

    public async Task RejectAsync(
        Guid leaveRequestId, string managerId, string? comment, CancellationToken ct = default)
    {
        // BR-013 (QA-M2): เหตุผลการ Reject เป็น optional — หัวหน้างานระบุหรือข้ามก็ได้
        var lr = await leaveRequestRepo.GetByIdAsync(leaveRequestId, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        if (lr.Status != LeaveStatus.Pending)
            throw new BusinessException($"ไม่สามารถปฏิเสธคำร้องที่มีสถานะ '{lr.Status}'", "INVALID_STATUS");

        if (lr.Employee.ManagerId != managerId)
            throw new BusinessException("ไม่มีสิทธิ์ปฏิเสธคำร้องนี้", "FORBIDDEN");

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            lr.Status          = LeaveStatus.Rejected;
            lr.RejectedBy      = managerId;
            lr.RejectedAt      = DateTime.UtcNow;
            lr.RejectionReason = comment;
            lr.UpdatedAt       = DateTime.UtcNow;
            lr.UpdatedBy       = managerId;
            leaveRequestRepo.Update(lr);

            // คืน PendingDays
            var balance = await balanceRepo.GetAsync(lr.EmployeeId, lr.LeaveTypeId, lr.StartDate.Year, ct);
            if (balance != null)
            {
                balance.PendingDays = Math.Max(0, balance.PendingDays - lr.DurationDays);
                balance.UpdatedAt   = DateTime.UtcNow;
                balance.UpdatedBy   = managerId;
                balanceRepo.Update(balance);
            }

            await approvalHistoryRepo.AddAsync(new ApprovalHistory
            {
                ApprovalHistoryId = Guid.NewGuid(),
                LeaveRequestId    = lr.LeaveRequestId,
                ApproverId        = managerId,
                Action            = ApprovalAction.Rejected,
                Reason            = comment,
                ActionAt          = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                CreatedBy         = managerId,
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    // ── Approve Cancel (SCR-007) ──────────────────────────────────────────────

    public async Task ApproveCancelAsync(
        Guid cancelRequestId, string managerId, string? comment, CancellationToken ct = default)
    {
        var cr = await cancelRequestRepo.GetByIdAsync(cancelRequestId, ct)
            ?? throw new NotFoundException(nameof(CancelRequest), cancelRequestId);

        if (cr.Status != CancelRequestStatus.Pending)
            throw new BusinessException("คำขอยกเลิกนี้ถูกดำเนินการแล้ว", "INVALID_STATUS");

        // VR-012 (BR-SF009-003): SLA หมดแล้วห้ามดำเนินการ — คำขอถูก escalate ไป HR
        if (cr.SlaDeadline < DateTime.UtcNow)
            throw new BusinessException("หมดเวลาดำเนินการ คำขอถูกส่งต่อให้ HR แล้ว", "SLA_EXPIRED");

        var lr = cr.LeaveRequest!;
        if (lr.Employee.ManagerId != managerId)
            throw new BusinessException("ไม่มีสิทธิ์อนุมัติคำขอยกเลิกนี้", "FORBIDDEN");

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            cr.Status    = CancelRequestStatus.Approved;
            cr.UpdatedAt = DateTime.UtcNow;
            cr.UpdatedBy = managerId;
            await cancelRequestRepo.UpdateAsync(cr, ct);

            lr.Status    = LeaveStatus.Cancelled;
            lr.UpdatedAt = DateTime.UtcNow;
            lr.UpdatedBy = managerId;
            leaveRequestRepo.Update(lr);

            // คืน UsedDays
            var balance = await balanceRepo.GetAsync(lr.EmployeeId, lr.LeaveTypeId, lr.StartDate.Year, ct);
            if (balance != null)
            {
                balance.UsedDays  = Math.Max(0, balance.UsedDays - lr.DurationDays);
                balance.UpdatedAt = DateTime.UtcNow;
                balance.UpdatedBy = managerId;
                balanceRepo.Update(balance);
            }

            await approvalHistoryRepo.AddAsync(new ApprovalHistory
            {
                ApprovalHistoryId = Guid.NewGuid(),
                CancelRequestId   = cr.CancelRequestId,
                ApproverId        = managerId,
                Action            = ApprovalAction.Approved,
                Reason            = comment,
                ActionAt          = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                CreatedBy         = managerId,
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    // ── Reject Cancel (SCR-007) ───────────────────────────────────────────────

    public async Task RejectCancelAsync(
        Guid cancelRequestId, string managerId, string? comment, CancellationToken ct = default)
    {
        // BR-SF009-004: การปฏิเสธคำขอยกเลิกต้องระบุเหตุผลเสมอ (ต่างจาก reject การลาที่ optional ตาม BR-013)
        if (string.IsNullOrWhiteSpace(comment))
            throw new BusinessException("กรุณาระบุเหตุผลในการปฏิเสธคำขอยกเลิก", "REJECTION_REASON_REQUIRED");

        var cr = await cancelRequestRepo.GetByIdAsync(cancelRequestId, ct)
            ?? throw new NotFoundException(nameof(CancelRequest), cancelRequestId);

        if (cr.Status != CancelRequestStatus.Pending)
            throw new BusinessException("คำขอยกเลิกนี้ถูกดำเนินการแล้ว", "INVALID_STATUS");

        // VR-012 (BR-SF009-003): SLA หมดแล้วห้ามดำเนินการ
        if (cr.SlaDeadline < DateTime.UtcNow)
            throw new BusinessException("หมดเวลาดำเนินการ คำขอถูกส่งต่อให้ HR แล้ว", "SLA_EXPIRED");

        var lr = cr.LeaveRequest!;
        if (lr.Employee.ManagerId != managerId)
            throw new BusinessException("ไม่มีสิทธิ์ปฏิเสธคำขอยกเลิกนี้", "FORBIDDEN");

        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            cr.Status    = CancelRequestStatus.Rejected;
            cr.UpdatedAt = DateTime.UtcNow;
            cr.UpdatedBy = managerId;
            await cancelRequestRepo.UpdateAsync(cr, ct);

            // คืน status กลับเป็น Approved
            lr.Status    = LeaveStatus.Approved;
            lr.UpdatedAt = DateTime.UtcNow;
            lr.UpdatedBy = managerId;
            leaveRequestRepo.Update(lr);

            await approvalHistoryRepo.AddAsync(new ApprovalHistory
            {
                ApprovalHistoryId = Guid.NewGuid(),
                CancelRequestId   = cr.CancelRequestId,
                ApproverId        = managerId,
                Action            = ApprovalAction.Rejected,
                Reason            = comment,
                ActionAt          = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                CreatedBy         = managerId,
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    // ── Pending by Manager (SCR-004) ──────────────────────────────────────────

    public async Task<PagedResult<PendingApprovalDto>> GetPendingByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await leaveRequestRepo.GetPendingByManagerAsync(managerId, page, pageSize, ct);
        var dtos = items.Select(r => new PendingApprovalDto(
            r.LeaveRequestId,
            r.LeaveRequestRef,
            r.EmployeeId,
            r.Employee.FullNameTh,
            r.LeaveType.TypeNameTh,
            r.StartDate,
            r.EndDate,
            r.DurationDays,
            r.Reason,
            r.CreatedAt
        )).ToList();
        return new PagedResult<PendingApprovalDto>
        { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    // ── Processed by Manager (SCR-004 Processed tab, SF-004) ──────────────────

    public async Task<PagedResult<HrLeaveRequestDto>> GetProcessedByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await leaveRequestRepo.GetProcessedByManagerAsync(managerId, page, pageSize, ct);
        var dtos = items.Select(r => new HrLeaveRequestDto(
            r.LeaveRequestId,
            r.LeaveRequestRef,
            r.EmployeeId,
            r.Employee.FullNameTh,
            r.Employee.Department,
            r.LeaveType.TypeNameTh,
            r.StartDate,
            r.EndDate,
            r.DurationDays,
            r.Status.ToString(),
            r.CreatedAt
        )).ToList();
        return new PagedResult<HrLeaveRequestDto>
        { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    // ── Cancel Requests by Manager (SCR-007) ──────────────────────────────────

    public async Task<PagedResult<PendingCancelRequestDto>> GetCancelRequestsByManagerAsync(
        string managerId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await leaveRequestRepo.GetCancelRequestsByManagerAsync(managerId, page, pageSize, ct);
        var dtos = items.Select(cr => new PendingCancelRequestDto(
            cr.CancelRequestId,
            cr.LeaveRequestId,
            cr.LeaveRequest!.LeaveRequestRef,
            cr.EmployeeId,
            cr.LeaveRequest.Employee.FullNameTh,
            cr.LeaveRequest.LeaveType.TypeNameTh,
            cr.LeaveRequest.StartDate,
            cr.LeaveRequest.EndDate,
            cr.LeaveRequest.DurationDays,
            cr.Reason,
            cr.CreatedAt,
            cr.SlaDeadline
        )).ToList();
        return new PagedResult<PendingCancelRequestDto>
        { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    // ── HR List (SCR-008) ─────────────────────────────────────────────────────

    public async Task<PagedResult<HrLeaveRequestDto>> GetAllForHrAsync(
        string? status, string? department, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await leaveRequestRepo.GetAllForHrAsync(status, department, page, pageSize, ct);
        var dtos = items.Select(r => new HrLeaveRequestDto(
            r.LeaveRequestId,
            r.LeaveRequestRef,
            r.EmployeeId,
            r.Employee.FullNameTh,
            r.Employee.Department,
            r.LeaveType.TypeNameTh,
            r.StartDate,
            r.EndDate,
            r.DurationDays,
            r.Status.ToString(),
            r.CreatedAt
        )).ToList();
        return new PagedResult<HrLeaveRequestDto>
        { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
