namespace LeaveRequest.Infrastructure.Adapters;

using System.Text;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// IF-001 Pattern A — parses a HRIS CSV batch file (UTF-8 with BOM, comma-delimited)
/// into <see cref="HrisEmployeeDto"/> records. Row-level errors are logged and skipped;
/// the whole file is never aborted due to a single bad row.
/// </summary>
public sealed class HrisBatchCsvAdapter : IHrisAdapter
{
    private readonly ILogger<HrisBatchCsvAdapter> _logger;

    public HrisBatchCsvAdapter(ILogger<HrisBatchCsvAdapter> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<HrisEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default) =>
        throw new NotSupportedException(
            $"{nameof(HrisBatchCsvAdapter)} handles IF-001 Pattern A (CSV batch). " +
            "Use HrisAdapter for Pattern B (REST API).");

    /// <summary>
    /// Streams <paramref name="csvStream"/> line-by-line (no full-file buffering),
    /// validates the header row, then maps each data row to <see cref="HrisEmployeeDto"/>.
    /// Invalid rows are skipped after logging a warning.
    /// </summary>
    /// <exception cref="FormatException">
    /// Thrown when the stream is empty or the header does not contain all required columns.
    /// </exception>
    public async Task<IEnumerable<HrisEmployeeDto>> ParseBatchFileAsync(
        Stream csvStream, CancellationToken ct = default)
    {
        // detectEncodingFromByteOrderMarks: true strips the UTF-8 BOM transparently.
        // leaveOpen: true — caller owns the stream lifetime.
        using var reader = new StreamReader(
            csvStream,
            encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true);

        var headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine))
            throw new FormatException("HRIS CSV is empty — no header row found.");

        var headers = SplitCsvLine(headerLine);
        HrisCsvValidator.ValidateHeader(headers);

        var colIndex = BuildColumnIndex(headers);

        var results = new List<HrisEmployeeDto>();
        var dataRowNumber = 0;
        var skippedCount = 0;

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            dataRowNumber++;

            try
            {
                var dto = MapRow(line, colIndex, dataRowNumber);
                results.Add(dto);
            }
            catch (Exception ex)
            {
                skippedCount++;
                _logger.LogWarning(
                    "IF-001 Pattern A: skipping data row {RowNumber} — {Error}",
                    dataRowNumber, ex.Message);
            }
        }

        _logger.LogInformation(
            "IF-001 Pattern A: finished — {Total} data rows, {Valid} valid, {Skipped} skipped.",
            dataRowNumber, results.Count, skippedCount);

        return results;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static Dictionary<string, int> BuildColumnIndex(string[] headers) =>
        headers
            .Select((h, i) => (Key: h.Trim().ToLowerInvariant(), Index: i))
            .ToDictionary(x => x.Key, x => x.Index);

    private static HrisEmployeeDto MapRow(
        string line, Dictionary<string, int> colIndex, int rowNumber)
    {
        var fields = SplitCsvLine(line);

        string Require(string col)
        {
            var value = Field(col);
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException(
                    $"Row {rowNumber}: required column '{col}' is empty.");
            return value;
        }

        string Field(string col)
        {
            if (!colIndex.TryGetValue(col, out var idx))
                throw new FormatException(
                    $"Row {rowNumber}: column '{col}' not found in header.");
            return idx < fields.Length ? fields[idx].Trim() : string.Empty;
        }

        string? Optional(string col)
        {
            var v = Field(col);
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        var hireDateRaw = Require("hire_date");
        if (!DateOnly.TryParseExact(hireDateRaw, "yyyy-MM-dd", out var hireDate))
            throw new FormatException(
                $"Row {rowNumber}: 'hire_date' value '{hireDateRaw}' is not yyyy-MM-dd.");

        var status = Field("employment_status");
        var isActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);

        return new HrisEmployeeDto(
            EmployeeId:   Require("employee_id"),
            EmployeeCode: Require("employee_code"),
            FullNameTh:   Require("name_th"),
            FullNameEn:   Require("name_en"),
            Department:   Optional("department"),
            Position:     Optional("position"),
            Email:        Require("email"),
            HireDate:     hireDate,
            ManagerId:    Optional("line_manager_id"),
            IsActive:     isActive
        );
    }

    /// <summary>
    /// RFC 4180 CSV line parser: handles quoted fields, embedded commas,
    /// and escaped double-quotes ("").
    /// </summary>
    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        fields.Add(sb.ToString());
                        sb.Clear();
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }

        fields.Add(sb.ToString());
        return [.. fields];
    }
}
