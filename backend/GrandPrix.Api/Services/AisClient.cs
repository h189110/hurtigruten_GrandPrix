using System.Diagnostics;
using System.Net.Http.Json;
using GrandPrix.Api.Configuration;
using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Models.Ais;
using Microsoft.Extensions.Options;

namespace GrandPrix.Api.Services;

public sealed class AisClientException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class AisClient(HttpClient httpClient, IOptions<AisSettings> settings, ILogger<AisClient> logger) : IAisClient
{
    public async Task<AisFeatureCollection> GetRealtimeFeaturesAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await httpClient.GetAsync(settings.Value.RealtimeGeoJsonPath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Kystverket AIS request failed with HTTP {StatusCode} after {ElapsedMs} ms",
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
                throw new AisClientException($"Kystverket responded with HTTP {(int)response.StatusCode}.");
            }

            var features = await response.Content.ReadFromJsonAsync<AisFeatureCollection>(cancellationToken);
            if (features is null)
            {
                throw new AisClientException("Kystverket returned an empty GeoJSON body.");
            }

            logger.LogDebug("Kystverket AIS request completed in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            return features;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Kystverket AIS request timed out after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            throw new AisClientException("Kystverket request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Kystverket AIS request failed after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            throw new AisClientException("Kystverket request failed.", exception);
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogWarning(exception, "Kystverket AIS response could not be deserialized");
            throw new AisClientException("Kystverket response could not be deserialized.", exception);
        }
    }
}
