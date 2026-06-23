namespace LeaveRequest.Application.Services;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;

public class LeaveTypeService(ILeaveTypeRepository repository, IUnitOfWork unitOfWork) : ILeaveTypeService
{
    public async Task<IReadOnlyList<LeaveTypeListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await repository.GetAllAsync(ct);
        return entities.Select(ToListItemDto).ToList();
    }

    public async Task<LeaveTypeDetailDto> GetByIdAsync(byte id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(LeaveType), id);
        return ToDetailDto(entity);
    }

    public async Task<LeaveTypeDetailDto> CreateAsync(CreateLeaveTypeRequest request, CancellationToken ct = default)
    {
        if (await repository.ExistsByCodeAsync(request.TypeCode, null, ct))
            throw new BusinessException($"TypeCode '{request.TypeCode}' already exists.", "DUPLICATE_TYPE_CODE");

        var entity = new LeaveType
        {
            TypeCode = request.TypeCode.ToUpperInvariant(),
            TypeNameTh = request.TypeNameTh,
            TypeNameEn = request.TypeNameEn,
            MaxDaysPerYear = request.MaxDaysPerYear,
            IsAvailableForOutsource = request.IsAvailableForOutsource,
            RequiresMedicalCert = request.RequiresMedicalCert,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM", // TODO: inject current user from ICurrentUserService
        };

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ToDetailDto(entity);
    }

    public async Task<LeaveTypeDetailDto> UpdateAsync(byte id, UpdateLeaveTypeRequest request, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(LeaveType), id);

        if (await repository.ExistsByCodeAsync(request.TypeCode, id, ct))
            throw new BusinessException($"TypeCode '{request.TypeCode}' already exists.", "DUPLICATE_TYPE_CODE");

        entity.TypeCode = request.TypeCode.ToUpperInvariant();
        entity.TypeNameTh = request.TypeNameTh;
        entity.TypeNameEn = request.TypeNameEn;
        entity.MaxDaysPerYear = request.MaxDaysPerYear;
        entity.IsAvailableForOutsource = request.IsAvailableForOutsource;
        entity.RequiresMedicalCert = request.RequiresMedicalCert;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = "SYSTEM"; // TODO: inject current user

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return ToDetailDto(entity);
    }

    public async Task DeleteAsync(byte id, string deletedBy, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(LeaveType), id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static LeaveTypeListItemDto ToListItemDto(LeaveType e) => new(
        e.LeaveTypeId,
        e.TypeCode,
        e.TypeNameTh,
        e.TypeNameEn,
        e.MaxDaysPerYear,
        e.IsAvailableForOutsource,
        e.RequiresMedicalCert
    );

    private static LeaveTypeDetailDto ToDetailDto(LeaveType e) => new(
        e.LeaveTypeId,
        e.TypeCode,
        e.TypeNameTh,
        e.TypeNameEn,
        e.MaxDaysPerYear,
        e.IsAvailableForOutsource,
        e.RequiresMedicalCert,
        e.CreatedAt,
        e.CreatedBy,
        e.UpdatedAt,
        e.UpdatedBy
    );
}
