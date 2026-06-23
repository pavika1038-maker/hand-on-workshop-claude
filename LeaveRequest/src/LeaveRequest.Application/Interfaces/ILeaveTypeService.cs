namespace LeaveRequest.Application.Interfaces;

using LeaveRequest.Application.DTOs;

public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveTypeListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<LeaveTypeDetailDto> GetByIdAsync(byte id, CancellationToken ct = default);
    Task<LeaveTypeDetailDto> CreateAsync(CreateLeaveTypeRequest request, CancellationToken ct = default);
    Task<LeaveTypeDetailDto> UpdateAsync(byte id, UpdateLeaveTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(byte id, string deletedBy, CancellationToken ct = default);
}
