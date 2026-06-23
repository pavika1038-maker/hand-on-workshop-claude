namespace LeaveRequest.Infrastructure.Adapters;

internal static class HrisCsvValidator
{
    // Expected columns from architecture doc §10.3 — order-insensitive, case-insensitive
    internal static readonly string[] RequiredColumns =
    [
        "employee_id",
        "employee_code",
        "name_th",
        "name_en",
        "department",
        "position",
        "email",
        "hire_date",
        "line_manager_id",
        "employment_status",
    ];

    /// <summary>
    /// Throws <see cref="FormatException"/> if any required column is absent from the CSV header.
    /// Column matching is case-insensitive.
    /// </summary>
    internal static void ValidateHeader(string[] actualHeaders)
    {
        var normalised = actualHeaders
            .Select(h => h.Trim().ToLowerInvariant())
            .ToHashSet();

        var missing = RequiredColumns
            .Where(c => !normalised.Contains(c))
            .ToArray();

        if (missing.Length > 0)
            throw new FormatException(
                $"HRIS CSV header validation failed — missing column(s): {string.Join(", ", missing)}. " +
                $"Expected: {string.Join(", ", RequiredColumns)}");
    }
}
