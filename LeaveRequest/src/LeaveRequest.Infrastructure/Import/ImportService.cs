namespace LeaveRequest.Infrastructure.Import;

using System.Text.Json;
using ClosedXML.Excel;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using Microsoft.Extensions.Logging;

/// <summary>
/// IF-003: Parse .xlsx, validate rows, upsert valid Outsource employees.
/// Rolls back all inserts when > 50% of rows fail validation.
/// ImportLog is always written regardless of success or failure.
/// </summary>
public sealed class ImportService : IImportService
{
    // Column header names (case-insensitive) — Assumption A-IF003-1
    private static readonly string[] RequiredColumns =
    [
        "employee_id", "employee_code", "full_name_th", "full_name_en",
        "email", "agency_company", "abc_start_date"
    ];

    private static readonly string[] OptionalColumns =
    [
        "department", "position", "manager_id"
    ];

    private readonly IEmployeeRepository _employeeRepo;
    private readonly IImportLogRepository _importLogRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        IEmployeeRepository employeeRepo,
        IImportLogRepository importLogRepo,
        AppDbContext context,
        ILogger<ImportService> logger)
    {
        _employeeRepo = employeeRepo;
        _importLogRepo = importLogRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportOutsourceEmployeesAsync(
        string hrEmployeeId,
        Stream excelStream,
        string fileName,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "IF-003 ImportOutsourceEmployees started. FileName={FileName} By={HrEmployeeId}",
            fileName, hrEmployeeId);

        // ── Step 1: Open workbook (validates it is a real .xlsx) ───────────────
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(excelStream);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IF-003 Failed to open Excel file. FileName={FileName}", fileName);
            throw new InvalidOperationException(
                $"ไม่สามารถเปิดไฟล์ Excel ได้ กรุณาตรวจสอบว่าเป็นไฟล์ .xlsx ที่ถูกต้อง ({fileName})", ex);
        }

        using (workbook)
        {
            var ws = workbook.Worksheets.First();

            // ── Step 2: Validate header row ────────────────────────────────────
            var headerMap = ParseHeaders(ws);

            // ── Step 3: Load existing emails (one DB round-trip for VR-013) ───
            var existingEmails = await _employeeRepo.GetAllEmailsAsync(ct);
            var batchEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var batchEmployeeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var validEmployees = new List<Employee>();
            var errors = new List<ImportErrorDto>();
            var failedRowNumbers = new HashSet<int>();

            // ── Step 4: Process data rows one by one ───────────────────────────
            var dataRows = ws.RowsUsed().Skip(1).ToList(); // header is row 1
            int rowNumber = 2;
            foreach (var row in dataRows)
            {
                var rowErrors = ValidateRow(
                    row, rowNumber, headerMap,
                    existingEmails, batchEmails, batchEmployeeIds);

                if (rowErrors.Count > 0)
                {
                    errors.AddRange(rowErrors);
                    failedRowNumbers.Add(rowNumber);
                }
                else
                {
                    var emp = MapRowToEmployee(row, headerMap, hrEmployeeId);
                    validEmployees.Add(emp);
                    batchEmails.Add(emp.Email);
                    batchEmployeeIds.Add(emp.EmployeeId);
                }

                rowNumber++;
            }

            int total = validEmployees.Count + failedRowNumbers.Count;
            int failedCount = failedRowNumbers.Count;
            double failedPct = total > 0 ? (double)failedCount / total : 0;
            bool isRolledBack = failedPct > 0.5;

            // ── Step 5a: Upsert valid employees (skip if >50% failure) ──────────
            int successCount = 0;
            if (!isRolledBack && validEmployees.Count > 0)
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    foreach (var emp in validEmployees)
                        await _employeeRepo.UpsertAsync(emp, ct);

                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    successCount = validEmployees.Count;

                    _logger.LogInformation(
                        "IF-003 Upserted {Count} outsource employees. FileName={FileName}",
                        successCount, fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "IF-003 Employee upsert failed. FileName={FileName} — rolling back.",
                        fileName);
                    isRolledBack = true;
                    _context.ChangeTracker.Clear(); // prevent re-save in next SaveChanges
                }
            }
            else if (isRolledBack)
            {
                _logger.LogWarning(
                    "IF-003 >50% rows failed ({Failed}/{Total}) — rollback entire file. FileName={FileName}",
                    failedCount, total, fileName);
            }

            // ── Step 5b: Always write ImportLog ─────────────────────────────────
            var importLogId = Guid.NewGuid();
            var log = new ImportLog
            {
                ImportLogId = importLogId,
                FileName = fileName,
                ImportedBy = hrEmployeeId,
                TotalRecords = total,
                SuccessRecords = successCount,
                FailedRecords = failedCount,
                ErrorDetailsJson = JsonSerializer.Serialize(errors),
                IsRolledBack = isRolledBack,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = hrEmployeeId,
            };
            await _importLogRepo.AddAsync(log, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "IF-003 Import completed. ImportLogId={ImportLogId} Total={Total} Success={Success} Failed={Failed} Rolled={Rolled}",
                importLogId, total, successCount, failedCount, isRolledBack);

            return new ImportResultDto(
                ImportLogId: importLogId,
                TotalRecords: total,
                SuccessRecords: successCount,
                FailedRecords: failedCount,
                IsRolledBack: isRolledBack,
                Errors: errors);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Dictionary<string, int> ParseHeaders(IXLWorksheet ws)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerRow = ws.Row(1);
        foreach (var cell in headerRow.CellsUsed())
        {
            var name = cell.Value.ToString().Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(name))
                map[name] = cell.Address.ColumnNumber;
        }

        var missing = RequiredColumns.Where(c => !map.ContainsKey(c)).ToList();
        if (missing.Count > 0)
            throw new FormatException(
                $"IF-003 Excel header ขาดคอลัมน์ที่จำเป็น: {string.Join(", ", missing)}");

        return map;
    }

    private static List<ImportErrorDto> ValidateRow(
        IXLRow row,
        int rowNumber,
        Dictionary<string, int> headerMap,
        HashSet<string> existingEmails,
        HashSet<string> batchEmails,
        HashSet<string> batchEmployeeIds)
    {
        var errors = new List<ImportErrorDto>();

        string Get(string col) => headerMap.TryGetValue(col, out var c)
            ? row.Cell(c).GetString().Trim()
            : string.Empty;

        var employeeId  = Get("employee_id");
        var employeeCode = Get("employee_code");
        var fullNameTh  = Get("full_name_th");
        var fullNameEn  = Get("full_name_en");
        var email       = Get("email");
        var agencyCompany = Get("agency_company");
        var abcStartDateStr = Get("abc_start_date");

        if (string.IsNullOrWhiteSpace(employeeId))
            errors.Add(new(rowNumber, "employee_id", "employee_id ห้ามว่าง"));
        else if (batchEmployeeIds.Contains(employeeId))
            errors.Add(new(rowNumber, "employee_id", $"employee_id '{employeeId}' ซ้ำกันในไฟล์"));

        if (string.IsNullOrWhiteSpace(employeeCode))
            errors.Add(new(rowNumber, "employee_code", "employee_code ห้ามว่าง"));

        if (string.IsNullOrWhiteSpace(fullNameTh))
            errors.Add(new(rowNumber, "full_name_th", "full_name_th ห้ามว่าง"));

        if (string.IsNullOrWhiteSpace(fullNameEn))
            errors.Add(new(rowNumber, "full_name_en", "full_name_en ห้ามว่าง"));

        // VR-013: email uniqueness
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(new(rowNumber, "email", "email ห้ามว่าง"));
        }
        else if (!IsValidEmail(email))
        {
            errors.Add(new(rowNumber, "email", $"email '{email}' ไม่ถูกรูปแบบ"));
        }
        else if (existingEmails.Contains(email))
        {
            errors.Add(new(rowNumber, "email", $"email '{email}' มีในระบบแล้ว (VR-013)"));
        }
        else if (batchEmails.Contains(email))
        {
            errors.Add(new(rowNumber, "email", $"email '{email}' ซ้ำกันในไฟล์ (VR-013)"));
        }

        if (string.IsNullOrWhiteSpace(agencyCompany))
            errors.Add(new(rowNumber, "agency_company", "agency_company ห้ามว่าง"));

        if (string.IsNullOrWhiteSpace(abcStartDateStr))
        {
            errors.Add(new(rowNumber, "abc_start_date", "abc_start_date ห้ามว่าง"));
        }
        else if (!DateOnly.TryParseExact(abcStartDateStr, "yyyy-MM-dd",
                     System.Globalization.CultureInfo.InvariantCulture,
                     System.Globalization.DateTimeStyles.None, out var abcDate))
        {
            errors.Add(new(rowNumber, "abc_start_date",
                $"abc_start_date '{abcStartDateStr}' ไม่ถูกรูปแบบ (ต้องเป็น yyyy-MM-dd)"));
        }
        else if (abcDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            errors.Add(new(rowNumber, "abc_start_date",
                $"abc_start_date '{abcStartDateStr}' ต้องไม่เกินวันนี้"));
        }

        return errors;
    }

    private static Employee MapRowToEmployee(
        IXLRow row,
        Dictionary<string, int> headerMap,
        string hrEmployeeId)
    {
        string Get(string col) => headerMap.TryGetValue(col, out var c)
            ? row.Cell(c).GetString().Trim()
            : string.Empty;

        string? GetOpt(string col)
        {
            var v = Get(col);
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        DateOnly.TryParseExact(
            Get("abc_start_date"), "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var abcStartDate);

        return new Employee
        {
            EmployeeId    = Get("employee_id"),
            EmployeeCode  = Get("employee_code"),
            FullNameTh    = Get("full_name_th"),
            FullNameEn    = Get("full_name_en"),
            Email         = Get("email"),
            AgencyCompany = Get("agency_company"),
            AbcStartDate  = abcStartDate,
            Department    = GetOpt("department"),
            Position      = GetOpt("position"),
            ManagerId     = GetOpt("manager_id"),
            EmployeeType  = EmployeeType.Outsource,
            IsActive      = true,
            HireDate      = abcStartDate,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = hrEmployeeId,
            UpdatedAt     = DateTime.UtcNow,
            UpdatedBy     = hrEmployeeId,
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
