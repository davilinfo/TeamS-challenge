using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using RpsLs.ApplicationService.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RpsLs.Tests;

public class RandomServiceTests
{
    private static RandomService CreateService(HttpMessageHandler handler) =>
        new(new HttpClient(handler), NullLogger<RandomService>.Instance);

    private static Mock<HttpMessageHandler> HandlerThatReturns(int number)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { random_number = number }),
            });
        return mock;
    }

    private static Mock<HttpMessageHandler> HandlerThatThrows() =>
        HandlerThatThrowsThenReturns(alwaysFail: true);

    private static Mock<HttpMessageHandler> HandlerThatThrowsThenReturns(
        bool alwaysFail = false, int successNumber = 42)
    {
        var mock = new Mock<HttpMessageHandler>();
        var seq = mock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        if (!alwaysFail)
            seq.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { random_number = successNumber }),
            });
        else
            seq.ThrowsAsync(new HttpRequestException("Service unavailable"));

        return mock;
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetRandomNumberAsync_ReturnsNumberFromApi(int number)
    {
        var sut = CreateService(HandlerThatReturns(number).Object);

        var result = await sut.GetRandomNumberAsync();

        Assert.Equal(number, result);
    }

    [Fact]
    public async Task GetRandomNumberAsync_ReturnsValueInValidRange_OnSuccess()
    {
        var sut = CreateService(HandlerThatReturns(37).Object);

        var result = await sut.GetRandomNumberAsync();

        Assert.InRange(result, 1, 100);
    }

    // ── Fallback ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRandomNumberAsync_ReturnsFallback_WhenApiAlwaysFails()
    {
        // Triggers up to 2 retry attempts (with 500 ms delay each) before fallback
        var sut = CreateService(HandlerThatThrows().Object);

        var result = await sut.GetRandomNumberAsync();

        Assert.InRange(result, 1, 100);
    }

    [Fact]
    public async Task GetRandomNumberAsync_ReturnsFallback_WhenApiReturnsNullBody()
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null"),
            });

        var sut = CreateService(mock.Object);

        var result = await sut.GetRandomNumberAsync();

        Assert.InRange(result, 1, 100);
    }

    // ── Retry ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRandomNumberAsync_ReturnsApiValue_WhenSucceedsAfterOneFailure()
    {
        // First call throws, second call succeeds with 42
        var sut = CreateService(HandlerThatThrowsThenReturns(successNumber: 42).Object);

        var result = await sut.GetRandomNumberAsync();

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetRandomNumberAsync_FallbackIsInValidRange_AfterExhaustedRetries()
    {
        var results = new List<int>();

        // Run multiple times to verify the fallback range is consistently valid
        for (int i = 0; i < 5; i++)
        {
            var sut = CreateService(HandlerThatThrows().Object);
            results.Add(await sut.GetRandomNumberAsync());
        }

        Assert.All(results, r => Assert.InRange(r, 1, 100));
    }
}
