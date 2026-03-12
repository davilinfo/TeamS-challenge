using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RpsLs.ApplicationService.Services;

public class RandomService(HttpClient httpClient, ILogger<RandomService> logger) : IRandomService
{
    private const string RandomEndpoint = "https://codechallenge.boohma.com/random";
    private const string retriesErrorMessage = "Failed to fetch random number from external service after retries; using local fallback";
    private const string retryWarningMessage = "Failed to fetch random number from external service; retrying...";
    private int retryAttempts = 2;
    private readonly int retryIntervalMs = 500;
    private static readonly int _minRandomNumber = 1;
    private static readonly int _maxRandomNumber = 101;

    public async Task<int> GetRandomNumberAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<RandomResponse>(RandomEndpoint);
            return response?.RandomNumber ?? FallbackRandom();
        }
        catch (Exception ex)
        {
            retryAttempts--;
            if (retryAttempts == 0)            {
                logger.LogError(ex, retriesErrorMessage);
                return FallbackRandom();
            }
            else
            {
                logger.LogWarning(ex, retryWarningMessage);
                await Task.Delay(retryIntervalMs);
                return await GetRandomNumberAsync();
            }   
        }
    }

    private static int FallbackRandom() => Random.Shared.Next(_minRandomNumber, _maxRandomNumber);

    private record RandomResponse(
        [property: JsonPropertyName("random_number")] int RandomNumber
    );
}
