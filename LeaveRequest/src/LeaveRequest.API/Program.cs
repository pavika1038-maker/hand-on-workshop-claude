using FluentValidation;
using FluentValidation.AspNetCore;
using LeaveRequest.API.Middleware;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Application.Services;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using LeaveRequest.Infrastructure.Extensions;
using LeaveRequest.Infrastructure.Import;
using LeaveRequest.Infrastructure.Repositories;
using LeaveRequest.Infrastructure.SlaScheduler;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .SelectMany(kv => kv.Value!.Errors.Select(e =>
                    $"{kv.Key}: {e.ErrorMessage}"))
                .ToList();

            var response = LeaveRequest.API.Models.ApiResponse<object>.Fail(
                "VALIDATION_ERROR",
                "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบ",
                details);

            return new Microsoft.AspNetCore.Mvc.UnprocessableEntityObjectResult(response);
        };
    });

// ── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Leave Request and Approval API",
        Version = "v1",
        Description = "ระบบบริหารการลาและการอนุมัติ — ABC Company"
    });
});

// ── EF Core (SQLite) ─────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("LeaveRequest.Infrastructure")));

// AppDbContext implements IUnitOfWork
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(LeaveTypeService).Assembly);

// ── Repository / Service DI ──────────────────────────────────────────────────
// COM-001 — LeaveType
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();

// SCR-001 — Leave Request submission
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IApprovalHistoryRepository, ApprovalHistoryRepository>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

// RPT-001 — Leave History Report
builder.Services.AddScoped<ILeaveReportRepository, LeaveReportRepository>();
builder.Services.AddScoped<ILeaveReportService, LeaveReportService>();

// ── HRIS Integration (IF-001) ────────────────────────────────────────────────
builder.Services.AddHrisIntegration(builder.Configuration);

// ── Excel Import (IF-003) ────────────────────────────────────────────────────
builder.Services.AddScoped<IImportLogRepository, ImportLogRepository>();
builder.Services.AddScoped<IImportService, ImportService>();

// ── Service Bus / Email Notification (IF-002) ────────────────────────────────
builder.Services.AddNotificationIntegration(builder.Configuration);

// ── SLA Timer Engine (IF-005) ─────────────────────────────────────────────────
builder.Services.AddScoped<ICancelRequestRepository, CancelRequestRepository>();
builder.Services.Configure<SlaSchedulerOptions>(
    builder.Configuration.GetSection(SlaSchedulerOptions.SectionName));
builder.Services.AddHostedService<SlaSchedulerService>();

// ── CORS (ให้ React Vite dev server เข้าถึงได้) ─────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
        policy.SetIsOriginAllowed(origin =>
                  new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Health Check ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Global Exception Middleware (ต้องอยู่ก่อน UseRouting) ──────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

// ── Swagger UI (Development only) ───────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Leave Request API v1"));
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();

// TODO: app.UseAuthentication(); app.UseAuthorization(); when auth is configured

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
