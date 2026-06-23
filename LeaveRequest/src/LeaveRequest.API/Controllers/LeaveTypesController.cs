namespace LeaveRequest.API.Controllers;

using LeaveRequest.API.Models;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/leave-types")]
public class LeaveTypesController(ILeaveTypeService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LeaveTypeListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<LeaveTypeListItemDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(byte id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<LeaveTypeDetailDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveTypeRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.LeaveTypeId },
            ApiResponse<LeaveTypeDetailDto>.Ok(result));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(byte id, [FromBody] UpdateLeaveTypeRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<LeaveTypeDetailDto>.Ok(result));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(byte id, CancellationToken ct)
    {
        // TODO: get deletedBy from authenticated user (HttpContext.User.Identity.Name)
        await service.DeleteAsync(id, "SYSTEM", ct);
        return NoContent();
    }
}
