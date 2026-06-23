namespace LeaveRequest.Domain.Exceptions;

public sealed class HrisAdapterException : Exception
{
    public int? StatusCode { get; }
    public string CorrelationId { get; }

    public HrisAdapterException(
        string message,
        int? statusCode = null,
        string? correlationId = null,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        CorrelationId = correlationId ?? string.Empty;
    }
}
