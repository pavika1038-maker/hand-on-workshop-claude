namespace LeaveRequest.Application.Tests.Adapters;

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Infrastructure.Adapters;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Polly.CircuitBreaker;
using Polly.Timeout;

public sealed class HrisAdapterTests
{
    // ── Shared fixtures ───────────────────────────────────────────────────────

    private static readonly IReadOnlyList<HrisEmployeeDto> SampleEmployees =
    [
        new("EMP001", "E001", "สมชาย ใจดี", "Somchai Jaidee",
            "IT", "Engineer", "somchai@abc.com", new DateOnly(2022, 1, 1), null, true)
    ];

    private static string ToJson(object value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

    // ── Builder ───────────────────────────────────────────────────────────────

    private static (HrisAdapter sut, Mock<IHrisTokenProvider> tokenMock) BuildSut(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> httpHandler)
    {
        var tokenMock = new Mock<IHrisTokenProvider>();
        tokenMock
            .Setup(x => x.GetTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("test-bearer-token");

        var options = Options.Create(new HrisOptions
        {
            BaseUrl           = "https://hris.test",
            EmployeesEndpoint = "/api/v1/employees"
        });

        var httpClient = new HttpClient(new FakeHttpHandler(httpHandler))
        {
            BaseAddress = new Uri("https://hris.test")
        };

        var sut = new HrisAdapter(
            httpClient,
            tokenMock.Object,
            options,
            NullLogger<HrisAdapter>.Instance);

        return (sut, tokenMock);
    }

    // ── Test 1: Happy Path ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllEmployeesAsync_HappyPath_ReturnsEmployeeList()
    {
        // Arrange
        var (sut, _) = BuildSut(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ToJson(SampleEmployees), Encoding.UTF8, "application/json")
            }));

        // Act
        var result = await sut.GetAllEmployeesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().EmployeeId.Should().Be("EMP001");
        result.First().Email.Should().Be("somchai@abc.com");
    }

    // ── Test 2: Timeout (Polly TimeoutRejectedException) ─────────────────────

    [Fact]
    public async Task GetAllEmployeesAsync_PollyTimeoutRejectedException_WrapsAsHrisAdapterException()
    {
        // Simulate Polly's timeout strategy throwing after per-attempt limit (10 s) is exceeded
        var (sut, _) = BuildSut(_ => throw new TimeoutRejectedException());

        // Act
        var act = async () => await sut.GetAllEmployeesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<HrisAdapterException>();
        ex.Which.InnerException.Should().BeOfType<TimeoutRejectedException>();
        ex.Which.Message.Should().Contain("timeout");
        ex.Which.CorrelationId.Should().NotBeNullOrEmpty();
        ex.Which.StatusCode.Should().BeNull();
    }

    // ── Test 3: Retry Exhausted (HttpRequestException after 3 retries) ────────

    [Fact]
    public async Task GetAllEmployeesAsync_HttpRequestExceptionAfterRetries_WrapsAsHrisAdapterException()
    {
        // Polly rethrows the last HttpRequestException once MaxRetryAttempts is exhausted
        var (sut, _) = BuildSut(_ =>
            throw new HttpRequestException("Connection refused", null, HttpStatusCode.ServiceUnavailable));

        // Act
        var act = async () => await sut.GetAllEmployeesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<HrisAdapterException>();
        ex.Which.InnerException.Should().BeOfType<HttpRequestException>();
        ex.Which.Message.Should().Contain("network");
        ex.Which.CorrelationId.Should().NotBeNullOrEmpty();
    }

    // ── Test 4: Circuit Open (Polly BrokenCircuitException) ──────────────────

    [Fact]
    public async Task GetAllEmployeesAsync_BrokenCircuitException_WrapsAsHrisAdapterException()
    {
        // Simulate Polly short-circuiting the call when the circuit breaker is open
        var (sut, _) = BuildSut(_ => throw new BrokenCircuitException("Circuit is open"));

        // Act
        var act = async () => await sut.GetAllEmployeesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<HrisAdapterException>();
        ex.Which.InnerException.Should().BeOfType<BrokenCircuitException>();
        ex.Which.Message.Should().Contain("circuit breaker");
        ex.Which.CorrelationId.Should().NotBeNullOrEmpty();
        ex.Which.StatusCode.Should().BeNull();
    }

    // ── Test 5: 401 Unauthorized ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllEmployeesAsync_401Unauthorized_ThrowsHrisAdapterExceptionWithStatusCode401()
    {
        // Polly does NOT retry 401 (not in ShouldHandle) — adapater checks status explicitly
        var (sut, _) = BuildSut(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));

        // Act
        var act = async () => await sut.GetAllEmployeesAsync();

        // Assert
        var ex = await act.Should().ThrowAsync<HrisAdapterException>();
        ex.Which.StatusCode.Should().Be(401);
        ex.Which.Message.Should().Contain("401");
        ex.Which.CorrelationId.Should().NotBeNullOrEmpty();
        ex.Which.InnerException.Should().BeNull();
    }

    // ── FakeHttpHandler ───────────────────────────────────────────────────────

    private sealed class FakeHttpHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request);
    }
}
