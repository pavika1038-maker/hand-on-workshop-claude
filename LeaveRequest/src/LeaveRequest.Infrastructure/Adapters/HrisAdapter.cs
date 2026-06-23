namespace LeaveRequest.Infrastructure.Adapters;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;

public sealed class HrisAdapter : IHrisAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IHrisTokenProvider _tokenProvider;
    private readonly HrisOptions _options;
    private readonly ILogger<HrisAdapter> _logger;

    public HrisAdapter(
        HttpClient httpClient,
        IHrisTokenProvider tokenProvider,
        IOptions<HrisOptions> options,
        ILogger<HrisAdapter> logger)
    {
        _httpClient    = httpClient;
        _tokenProvider = tokenProvider;
        _options       = options.Value;
        _logger        = logger;
    }

    public async Task<IEnumerable<HrisEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];

        try
        {
            var token = await _tokenProvider.GetTokenAsync(ct);

            using var request = new HttpRequestMessage(HttpMethod.Get, _options.EmployeesEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

            _logger.LogInformation(
                "IF-001 GetAllEmployees → HRIS. CorrelationId={CorrelationId}",
                correlationId);

            using var response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning(
                    "HRIS rejected request with 401 Unauthorized. CorrelationId={CorrelationId}",
                    correlationId);
                throw new HrisAdapterException(
                    "HRIS API rejected the bearer token (401 Unauthorized) — verify client credentials and token endpoint",
                    statusCode: 401,
                    correlationId: correlationId);
            }

            if (!response.IsSuccessStatusCode)
            {
                var code = (int)response.StatusCode;
                _logger.LogWarning(
                    "HRIS returned non-success status {StatusCode}. CorrelationId={CorrelationId}",
                    code, correlationId);
                throw new HrisAdapterException(
                    $"HRIS API returned HTTP {code}",
                    statusCode: code,
                    correlationId: correlationId);
            }

            // NOT logging response body — contains PII (ISO 27001 / PDPA)
            var employees = await response.Content.ReadFromJsonAsync<List<HrisEmployeeDto>>(ct) ?? [];

            _logger.LogInformation(
                "IF-001 HRIS sync complete — {Count} employees. CorrelationId={CorrelationId}",
                employees.Count, correlationId);

            return employees;
        }
        catch (HrisAdapterException)
        {
            throw; // already wrapped — pass through
        }
        catch (BrokenCircuitException ex)
        {
            // WRN-IF001-001: Polly opened circuit after reaching failure threshold
            _logger.LogWarning(
                "WRN-IF001-001 HRIS circuit breaker OPEN. CorrelationId={CorrelationId}",
                correlationId);
            throw new HrisAdapterException(
                "HRIS API is currently unavailable (circuit breaker open — retry after 30 s)",
                correlationId: correlationId,
                inner: ex);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(
                ex,
                "HRIS read timeout exceeded (10 s per attempt). CorrelationId={CorrelationId}",
                correlationId);
            throw new HrisAdapterException(
                "HRIS API did not respond within the read timeout (10 s)",
                correlationId: correlationId,
                inner: ex);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // TCP connection timeout (SocketsHttpHandler.ConnectTimeout = 5 s)
            _logger.LogError(
                ex,
                "HRIS TCP connection timeout (5 s). CorrelationId={CorrelationId}",
                correlationId);
            throw new HrisAdapterException(
                "HRIS API connection timed out (5 s)",
                correlationId: correlationId,
                inner: ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HRIS HTTP request failed (retries exhausted). CorrelationId={CorrelationId}",
                correlationId);
            throw new HrisAdapterException(
                "HRIS API request failed — network or DNS error",
                correlationId: correlationId,
                inner: ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error calling HRIS. CorrelationId={CorrelationId}",
                correlationId);
            throw new HrisAdapterException(
                "Unexpected error during HRIS API call",
                correlationId: correlationId,
                inner: ex);
        }
    }

    // IF-001 Pattern A (batch CSV) — not implemented in this Pattern B adapter
    public Task<IEnumerable<HrisEmployeeDto>> ParseBatchFileAsync(
        Stream csvStream, CancellationToken ct = default) =>
        throw new NotSupportedException(
            $"{nameof(HrisAdapter)} handles IF-001 Pattern B (REST API). " +
            "Use a dedicated Pattern A adapter for batch CSV import.");
}
