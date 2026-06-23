using FluentValidation;
using LeaveRequest.API.Models;
using LeaveRequest.Domain.Exceptions;

namespace LeaveRequest.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message);
        }
        catch (BusinessException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status422UnprocessableEntity, ex.Code, ex.Message);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors.Select(e => e.ErrorMessage);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, "VALIDATION_ERROR", "ข้อมูลไม่ถูกต้อง", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "เกิดข้อผิดพลาดภายในระบบ");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context, int statusCode, string code, string message,
        IEnumerable<string>? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail(code, message, details);
        await context.Response.WriteAsJsonAsync(response);
    }
}
