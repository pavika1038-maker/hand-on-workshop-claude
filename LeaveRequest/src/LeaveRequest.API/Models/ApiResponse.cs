namespace LeaveRequest.API.Models;

/// <summary>
/// Standard API envelope — flat structure matches frontend TypeScript ApiResponse&lt;T&gt; contract.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }

    // ── Error fields (flat — matches frontend interface) ──────────────────────
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public PaginationMeta? Metadata { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string code, string message, IEnumerable<string>? details = null)
        => new()
        {
            Success   = false,
            ErrorCode = code,
            Message   = message,
            Errors    = details?.ToList()
        };
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
