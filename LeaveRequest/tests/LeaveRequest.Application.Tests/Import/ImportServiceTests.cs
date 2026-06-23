namespace LeaveRequest.Application.Tests.Import;

using System.Text;
using ClosedXML.Excel;
using FluentAssertions;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Data;
using LeaveRequest.Infrastructure.Import;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

public sealed class ImportServiceTests : IDisposable
{
    // ── Fixtures ───────────────────────────────────────────────────────────────

    private const string HrEmployeeId = "HR001";

    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IEmployeeRepository> _empRepoMock = new();
    private readonly Mock<IImportLogRepository> _logRepoMock = new();

    public ImportServiceTests()
    {
        // SQLite InMemory supports real transactions (unlike EF InMemory provider)
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        // Default: no existing emails in DB
        _empRepoMock
            .Setup(r => r.GetAllEmailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        // UpsertAsync: no-op in mock (context transaction still commits empty)
        _empRepoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // AddAsync ImportLog: no-op in mock
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private ImportService BuildSut() => new(
        _empRepoMock.Object,
        _logRepoMock.Object,
        _context,
        NullLogger<ImportService>.Instance);

    // ── Excel stream builder ───────────────────────────────────────────────────

    private static Stream BuildValidExcel(IEnumerable<string[]>? extraRows = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Employees");

        ws.Cell(1, 1).Value = "employee_id";
        ws.Cell(1, 2).Value = "employee_code";
        ws.Cell(1, 3).Value = "full_name_th";
        ws.Cell(1, 4).Value = "full_name_en";
        ws.Cell(1, 5).Value = "email";
        ws.Cell(1, 6).Value = "agency_company";
        ws.Cell(1, 7).Value = "abc_start_date";
        ws.Cell(1, 8).Value = "department";
        ws.Cell(1, 9).Value = "position";
        ws.Cell(1, 10).Value = "manager_id";

        var rows = extraRows?.ToList() ?? new List<string[]>
        {
            new[] {"OS001", "OS001", "สมชาย ใจดี", "Somchai Jaidee",
                   "somchai@outsource.com", "ABC Outsource Co.", "2025-01-01",
                   "IT", "Developer", "EMP001"}
        };

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            for (int j = 0; j < row.Length; j++)
                ws.Cell(i + 2, j + 1).Value = row[j];
        }

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    // ── Test 1: Happy path — all rows pass, ImportLog written ─────────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_ValidFile_ImportsAllRowsAndWritesLog()
    {
        var sut = BuildSut();
        await using var stream = BuildValidExcel();

        var result = await sut.ImportOutsourceEmployeesAsync(HrEmployeeId, stream, "employees.xlsx");

        result.TotalRecords.Should().Be(1);
        result.SuccessRecords.Should().Be(1);
        result.FailedRecords.Should().Be(0);
        result.IsRolledBack.Should().BeFalse();
        result.Errors.Should().BeEmpty();

        _empRepoMock.Verify(r => r.UpsertAsync(
            It.Is<Employee>(e => e.EmployeeId == "OS001" && e.Email == "somchai@outsource.com"),
            It.IsAny<CancellationToken>()), Times.Once);

        _logRepoMock.Verify(r => r.AddAsync(
            It.Is<ImportLog>(l => l.SuccessRecords == 1 && l.FailedRecords == 0 && !l.IsRolledBack),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 2: Duplicate email within file — VR-013 ──────────────────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_DuplicateEmailInFile_ReportsVr013Error()
    {
        var sut = BuildSut();
        await using var stream = BuildValidExcel(new string[][]
        {
            ["OS001", "OS001", "สมชาย ใจดี", "Somchai Jaidee",
             "dup@outsource.com", "Agency A", "2025-01-01", "", "", ""],

            ["OS002", "OS002", "สมหญิง ดีงาม", "Somying Deengam",
             "dup@outsource.com", "Agency A", "2025-01-01", "", "", ""],  // same email
        });

        var result = await sut.ImportOutsourceEmployeesAsync(HrEmployeeId, stream, "employees.xlsx");

        result.TotalRecords.Should().Be(2);
        result.SuccessRecords.Should().Be(1);
        result.FailedRecords.Should().Be(1);
        result.IsRolledBack.Should().BeFalse("1/2 = 50% is not > 50%");
        result.Errors.Should().ContainSingle(e =>
            e.Field == "email" && e.Message.Contains("VR-013"));

        _logRepoMock.Verify(r => r.AddAsync(
            It.Is<ImportLog>(l => l.FailedRecords == 1 && !l.IsRolledBack),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 3: Non-Excel stream → InvalidOperationException ──────────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_NotExcelStream_ThrowsInvalidOperationException()
    {
        var sut = BuildSut();
        await using var garbage = new MemoryStream(Encoding.UTF8.GetBytes("not an xlsx file"));

        var act = async () => await sut.ImportOutsourceEmployeesAsync(
            HrEmployeeId, garbage, "bad.xlsx");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ไม่สามารถเปิดไฟล์ Excel*");
    }

    // ── Test 4: >50% rows fail → rollback entire file ─────────────────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_MajorityFailValidation_RollsBackAndWritesLog()
    {
        var sut = BuildSut();

        // 1 valid, 2 invalid → 2/3 ≈ 66.7% > 50%
        await using var stream = BuildValidExcel(new string[][]
        {
            ["OS001", "OS001", "ชื่อ นามสกุล", "Name Surname",
             "valid@outsource.com", "Agency", "2025-01-01", "", "", ""],

            ["", "OS002", "ชื่อ สอง", "Name Two",       // missing employee_id
             "two@outsource.com", "Agency", "2025-01-01", "", "", ""],

            ["OS003", "", "ชื่อ สาม", "Name Three",       // missing employee_code
             "three@outsource.com", "Agency", "2025-01-01", "", "", ""],
        });

        var result = await sut.ImportOutsourceEmployeesAsync(HrEmployeeId, stream, "employees.xlsx");

        result.IsRolledBack.Should().BeTrue("2/3 rows failed > 50%");
        result.SuccessRecords.Should().Be(0, "rollback means no records imported");
        result.FailedRecords.Should().Be(2);
        result.TotalRecords.Should().Be(3);

        _empRepoMock.Verify(r => r.UpsertAsync(
            It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);

        _logRepoMock.Verify(r => r.AddAsync(
            It.Is<ImportLog>(l => l.IsRolledBack && l.SuccessRecords == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 5: Email exists in DB — VR-013 ───────────────────────────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_EmailExistsInDb_ReportsVr013Error()
    {
        _empRepoMock
            .Setup(r => r.GetAllEmailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(["existing@outsource.com"],
                StringComparer.OrdinalIgnoreCase));

        var sut = BuildSut();
        await using var stream = BuildValidExcel(new string[][]
        {
            ["OS001", "OS001", "สมชาย ใจดี", "Somchai Jaidee",
             "existing@outsource.com", "Agency", "2025-01-01", "", "", ""],
        });

        var result = await sut.ImportOutsourceEmployeesAsync(HrEmployeeId, stream, "employees.xlsx");

        result.FailedRecords.Should().Be(1);
        result.Errors.Should().ContainSingle(e =>
            e.Field == "email" && e.Message.Contains("VR-013"));
    }

    // ── Test 6: Missing required header column → FormatException ──────────────

    [Fact]
    public async Task ImportOutsourceEmployeesAsync_MissingRequiredHeader_ThrowsFormatException()
    {
        var sut = BuildSut();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "employee_id";  // only one column — others missing

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var act = async () => await sut.ImportOutsourceEmployeesAsync(
            HrEmployeeId, ms, "bad-header.xlsx");

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("*IF-003 Excel header ขาดคอลัมน์*");
    }
}
