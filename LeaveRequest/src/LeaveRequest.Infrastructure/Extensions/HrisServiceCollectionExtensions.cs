namespace LeaveRequest.Infrastructure.Extensions;

using LeaveRequest.Application.Interfaces;
using LeaveRequest.Infrastructure.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Threading.RateLimiting;

public static class HrisServiceCollectionExtensions
{
    /// <summary>
    /// Registers IF-001 HRIS adapter. Reads "Hris" section from configuration.
    /// When UseStub=true uses HrisAdapterStub; otherwise registers HrisAdapter with
    /// full Polly resilience pipeline (bulkhead → retry → circuit breaker → timeout).
    /// </summary>
    public static IServiceCollection AddHrisIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HrisOptions>(configuration.GetSection(HrisOptions.SectionName));

        var hrisConfig = configuration
            .GetSection(HrisOptions.SectionName)
            .Get<HrisOptions>() ?? new HrisOptions();

        if (hrisConfig.UseStub)
        {
            services.AddScoped<IHrisAdapter, HrisAdapterStub>();
            return services;
        }

        // Named HttpClient for OAuth 2.0 token endpoint (no Polly — fast, self-recovering)
        services.AddHttpClient("hris-token", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<IHrisTokenProvider, HrisTokenClient>();

        // Typed HttpClient for HRIS API with Polly resilience pipeline:
        //   Bulkhead (25) → Retry (3×, 2 s/4 s/8 s) → CircuitBreaker (5 fail → 30 s) → Timeout (10 s/attempt)
        services
            .AddHttpClient<IHrisAdapter, HrisAdapter>(client =>
            {
                client.BaseAddress = new Uri(hrisConfig.BaseUrl);
                client.Timeout     = Timeout.InfiniteTimeSpan; // Polly controls timeouts
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                ConnectTimeout          = TimeSpan.FromSeconds(5),  // TCP connect limit (separate from read)
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            })
            .AddResilienceHandler("hris-resilience", pipeline =>
            {
                // Bulkhead — reject when > 25 concurrent HRIS calls are in-flight
                pipeline.AddRateLimiter(new ConcurrencyLimiter(
                    new ConcurrencyLimiterOptions { PermitLimit = 25, QueueLimit = 0 }));

                // Retry — 3× exponential backoff: 2 s → 4 s → 8 s
                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay            = TimeSpan.FromSeconds(2),
                    BackoffType      = DelayBackoffType.Exponential,
                    UseJitter        = false
                });

                // Circuit Breaker — open after ≥50 % failures (min 5 calls in 60 s window); reset after 30 s
                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio      = 0.5,
                    MinimumThroughput = 5,
                    SamplingDuration  = TimeSpan.FromSeconds(60),
                    BreakDuration     = TimeSpan.FromSeconds(30)
                });

                // Per-attempt read timeout: 10 s
                pipeline.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }
}
