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
    IUnitOfWork unitOfWork,
    ILogger<LeaveRequestService> logger
) : ILeaveRequestService
{
    // ── Submit (SFR-003) ──────────────────────────────────────────────────────

    public async Task<LeaveRequestResult> SubmitLeaveRequestAsync(
        string employeeId, CreateLeaveRequestDto request, CancellationToken ct = default)
    {
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
            r.CreatedAt
        );
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

        if (lr.Employee.ManagerId != managerId)
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
        var cr = await cancelRequestRepo.GetByIdAsync(cancelRequestId, ct)
            ?? throw new NotFoundException(nameof(CancelRequest), cancelRequestId);

        if (cr.Status != CancelRequestStatus.Pending)
            throw new BusinessException("คำขอยกเลิกนี้ถูกดำเนินการแล้ว", "INVALID_STATUS");

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
