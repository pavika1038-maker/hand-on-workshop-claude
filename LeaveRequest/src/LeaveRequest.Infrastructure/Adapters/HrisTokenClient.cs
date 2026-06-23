namespace LeaveRequest.Infrastructure.Adapters;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LeaveRequest.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class HrisTokenClient : IHrisTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HrisOptions _options;
    private readonly ILogger<HrisTokenClient> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public HrisTokenClient(
        IHttpClientFactory httpClientFactory,
        IOptions<HrisOptions> options,
        ILogger<HrisTokenClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock (another thread may have already refreshed)
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _cachedToken;

            return await FetchNewTokenAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> FetchNewTokenAsync(CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient("hris-token");

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"]  = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"]      = _options.Scope
        };

        using var response = await client.PostAsync(
            _options.TokenEndpoint,
            new FormUrlEncodedContent(form),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "HRIS token endpoint returned HTTP {StatusCode}",
                (int)response.StatusCode);
            throw new HrisAdapterException(
                $"Failed to obtain HRIS access token (HTTP {(int)response.StatusCode})",
                statusCode: (int)response.StatusCode);
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<HrisTokenResponse>(ct)
            ?? throw new HrisAdapterException("HRIS token endpoint returned empty response");

        _cachedToken = tokenResponse.AccessToken;
        // Buffer 60 s before actual expiry to prevent using a near-expired token
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

        // Log only that the token was acquired — NEVER log the token value (ISO 27001 / PDPA)
        _logger.LogInformation(
            "HRIS OAuth token acquired (expires_in={ExpiresIn}s)",
            tokenResponse.ExpiresIn);

        return _cachedToken;
    }

    private sealed record HrisTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")]   int    ExpiresIn,
        [property: JsonPropertyName("token_type")]   string TokenType
    );
}
