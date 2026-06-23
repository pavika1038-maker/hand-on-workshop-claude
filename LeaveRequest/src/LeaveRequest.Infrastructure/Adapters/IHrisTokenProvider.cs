namespace LeaveRequest.Infrastructure.Adapters;

public interface IHrisTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}
