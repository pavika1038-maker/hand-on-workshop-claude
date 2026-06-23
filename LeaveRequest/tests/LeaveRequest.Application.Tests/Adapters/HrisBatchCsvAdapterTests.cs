namespace LeaveRequest.Application.Tests.Adapters;

using System.Text;
using FluentAssertions;
using LeaveRequest.Infrastructure.Adapters;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class HrisBatchCsvAdapterTests
{
    // ── Shared fixtures ───────────────────────────────────────────────────────

    private const string ValidHeader =
        "employee_id,employee_code,name_th,name_en,department,position,email,hire_date,line_manager_id,employment_status";

    private const string ValidRow1 =
        "\"EMP001\",\"EMP001\",\"สมชาย ใจดี\",\"Somchai Jaidee\",\"Information Technology\",\"Software Engineer\",\"somchai@abc.com\",\"2022-01-15\",\"EMP050\",\"Active\"";

    // ManagerId and Department intentionally empty (optional fields)
    private const string ValidRow2 =
        "\"EMP002\",\"EMP002\",\"สมหญิง รักดี\",\"Somying Rakdee\",\"\",\"HR Manager\",\"somying@abc.com\",\"2021-03-01\",,\"Inactive\"";

    private static HrisBatchCsvAdapter CreateSut() =>
        new(NullLogger<HrisBatchCsvAdapter>.Instance);

    /// <summary>Creates an in-memory stream from <paramref name="content"/>.</summary>
    private static Stream ToStream(string content, bool withBom = false)
    {
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: withBom);
        return new MemoryStream(encoding.GetBytes(content));
    }

    // ── Test 1: Valid CSV — all rows parsed correctly ─────────────────────────

    [Fact]
    public async Task ParseBatchFileAsync_ValidCsv_ReturnsAllMappedRows()
    {
        var csv = string.Join("\n", ValidHeader, ValidRow1, ValidRow2);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv))).ToList();

        result.Should().HaveCount(2);

        var first = result[0];
        first.EmployeeId.Should().Be("EMP001");
        first.EmployeeCode.Should().Be("EMP001");
        first.FullNameTh.Should().Be("สมชาย ใจดี");
        first.FullNameEn.Should().Be("Somchai Jaidee");
        first.Department.Should().Be("Information Technology");
        first.Email.Should().Be("somchai@abc.com");
        first.HireDate.Should().Be(new DateOnly(2022, 1, 15));
        first.ManagerId.Should().Be("EMP050");
        first.IsActive.Should().BeTrue();

        var second = result[1];
        second.EmployeeId.Should().Be("EMP002");
        second.Department.Should().BeNull();   // empty field → null
        second.ManagerId.Should().BeNull();    // empty field → null
        second.IsActive.Should().BeFalse();    // "Inactive" → false
    }

    // ── Test 2: UTF-8 with BOM — BOM stripped, header parsed correctly ────────

    [Fact]
    public async Task ParseBatchFileAsync_Utf8WithBom_ParsesHeaderWithoutBomPrefix()
    {
        // BOM bytes at start of stream must not corrupt the first column name ("employee_id")
        var csv = string.Join("\n", ValidHeader, ValidRow1);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv, withBom: true))).ToList();

        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be("EMP001");
    }

    // ── Test 3: Missing required header column → FormatException ─────────────

    [Fact]
    public async Task ParseBatchFileAsync_MissingHeaderColumn_ThrowsFormatException()
    {
        // 'employment_status' column is absent
        const string incompleteHeader =
            "employee_id,employee_code,name_th,name_en,department,position,email,hire_date,line_manager_id";
        var csv = incompleteHeader + "\n" + "\"EMP001\",\"EMP001\",\"สมชาย\",\"Somchai\",\"IT\",\"Eng\",\"a@b.com\",\"2022-01-01\",\"EMP050\"";
        var sut = CreateSut();

        var act = async () => await sut.ParseBatchFileAsync(ToStream(csv));

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("*employment_status*");
    }

    // ── Test 4: Malformed row (invalid date) → row skipped, valid rows returned

    [Fact]
    public async Task ParseBatchFileAsync_RowWithInvalidDate_SkipsRowAndReturnsRemainingRows()
    {
        const string badDateRow =
            "\"EMP002\",\"EMP002\",\"สมหญิง\",\"Somying\",\"IT\",\"Eng\",\"s@b.com\",\"not-a-date\",\"\",\"Active\"";
        var csv = string.Join("\n", ValidHeader, ValidRow1, badDateRow);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv))).ToList();

        // Bad row is silently skipped — only valid row 1 returned
        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be("EMP001");
    }

    // ── Test 5: Row with empty required field → row skipped ──────────────────

    [Fact]
    public async Task ParseBatchFileAsync_RowWithEmptyRequiredField_SkipsRow()
    {
        // employee_id is empty string
        const string emptyIdRow =
            "\"\",\"EMP002\",\"สมหญิง\",\"Somying\",\"IT\",\"Eng\",\"s@b.com\",\"2022-01-01\",\"\",\"Active\"";
        var csv = string.Join("\n", ValidHeader, ValidRow1, emptyIdRow);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv))).ToList();

        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be("EMP001");
    }

    // ── Test 6: Empty file → FormatException ─────────────────────────────────

    [Fact]
    public async Task ParseBatchFileAsync_EmptyFile_ThrowsFormatException()
    {
        var sut = CreateSut();

        var act = async () => await sut.ParseBatchFileAsync(ToStream(string.Empty));

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("*empty*");
    }

    // ── Test 7: Header only, no data rows → empty result ─────────────────────

    [Fact]
    public async Task ParseBatchFileAsync_HeaderOnlyNoDataRows_ReturnsEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.ParseBatchFileAsync(ToStream(ValidHeader));

        result.Should().BeEmpty();
    }

    // ── Test 8: Multiple malformed rows scattered between valid rows ───────────

    [Fact]
    public async Task ParseBatchFileAsync_MixedValidAndMalformedRows_ReturnsOnlyValidRows()
    {
        const string badRow =
            "\"EMP999\",\"EMP999\",\"ผิดพลาด\",\"Error\",\"IT\",\"Eng\",\"err@b.com\",\"BADDATE\",\"\",\"Active\"";
        var csv = string.Join("\n", ValidHeader, ValidRow1, badRow, ValidRow2, badRow);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv))).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.EmployeeId).Should().BeEquivalentTo(["EMP001", "EMP002"]);
    }

    // ── Test 9: Quoted fields containing commas are parsed correctly ──────────

    [Fact]
    public async Task ParseBatchFileAsync_QuotedFieldWithComma_ParsesCorrectly()
    {
        // Department value contains a comma inside quotes
        const string rowWithCommaInField =
            "\"EMP001\",\"EMP001\",\"สมชาย ใจดี\",\"Somchai Jaidee\",\"IT, Operations\",\"Engineer\",\"somchai@abc.com\",\"2022-01-15\",\"\",\"Active\"";
        var csv = string.Join("\n", ValidHeader, rowWithCommaInField);
        var sut = CreateSut();

        var result = (await sut.ParseBatchFileAsync(ToStream(csv))).ToList();

        result.Should().HaveCount(1);
        result[0].Department.Should().Be("IT, Operations");
    }

    // ── Test 10: GetAllEmployeesAsync → NotSupportedException ────────────────

    [Fact]
    public async Task GetAllEmployeesAsync_AlwaysThrowsNotSupportedException()
    {
        var sut = CreateSut();

        var act = async () => await sut.GetAllEmployeesAsync();

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Pattern B*");
    }
}
