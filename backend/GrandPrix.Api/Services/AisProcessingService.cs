using System.Globalization;
using System.Text.Json;
using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Models;
using GrandPrix.Api.Models.Ais;
using GrandPrix.Api.Models.Vessels;

namespace GrandPrix.Api.Services;

public sealed class AisProcessingService(ILogger<AisProcessingService> logger) : IAisProcessingService
{
    public AisProcessingResult Process(AisFeatureCollection featureCollection, DateTimeOffset retrievedAtUtc)
    {
        var observationsByMmsi = new Dictionary<long, VesselObservation>();
        var matchedMmsi = new HashSet<long>();
        var issues = new List<string>();
        var identityMismatchCount = 0;

        foreach (var feature in featureCollection.Features)
        {
            var properties = feature.Properties;
            if (properties?.Mmsi is not { } mmsi)
            {
                issues.Add("Feature without MMSI was ignored.");
                continue;
            }

            var vessel = VesselRegistry.Vessels.FirstOrDefault(v => v.Mmsi == mmsi);
            if (vessel is null)
            {
                continue;
            }

            matchedMmsi.Add(mmsi);

            if (properties.Imo is { } imo && imo != vessel.Imo)
            {
                identityMismatchCount++;
                var message = $"Identity mismatch for {vessel.Name} (MMSI {mmsi}): AIS IMO {imo}, registry IMO {vessel.Imo}.";
                issues.Add(message);
                logger.LogWarning("AIS identity mismatch for MMSI {Mmsi}: AIS IMO {AisImo}, registry IMO {RegistryImo}", mmsi, imo, vessel.Imo);
                continue;
            }

            if (properties.Speed is not { } speed || !double.IsFinite(speed) || speed < 0)
            {
                issues.Add($"Missing or invalid speed for {vessel.Name} (MMSI {mmsi}).");
                continue;
            }

            if (!TryGetLatestPosition(feature.Geometry, out var latitude, out var longitude))
            {
                issues.Add($"Missing or invalid coordinates for {vessel.Name} (MMSI {mmsi}).");
                continue;
            }

            if (!TryParseSourceTimestamp(properties.DateTimeUtc, out var sourceTimestampUtc))
            {
                issues.Add($"Missing or invalid timestamp for {vessel.Name} (MMSI {mmsi}).");
                continue;
            }

            var observation = new VesselObservation(
                vessel,
                speed,
                latitude,
                longitude,
                properties.CourseOverGround,
                properties.TrueHeading,
                sourceTimestampUtc,
                retrievedAtUtc);

            if (observationsByMmsi.TryGetValue(mmsi, out var existing))
            {
                issues.Add($"Duplicate feed entries for {vessel.Name} (MMSI {mmsi}).");
                if (existing.SourceTimestampUtc >= sourceTimestampUtc)
                {
                    continue;
                }
            }

            observationsByMmsi[mmsi] = observation;
        }

        return new AisProcessingResult(
            featureCollection.Features.Count,
            matchedMmsi.Count,
            observationsByMmsi.Values.ToList(),
            issues,
            identityMismatchCount);
    }

    public static bool TryGetLatestPosition(AisGeometry? geometry, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        if (geometry?.Coordinates.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        JsonElement point;
        switch (geometry.Type)
        {
            case "LineString":
                var count = geometry.Coordinates.GetArrayLength();
                if (count == 0)
                {
                    return false;
                }

                point = geometry.Coordinates[count - 1];
                break;
            case "Point":
                point = geometry.Coordinates;
                break;
            default:
                return false;
        }

        if (point.ValueKind != JsonValueKind.Array
            || point.GetArrayLength() < 2
            || point[0].ValueKind != JsonValueKind.Number
            || point[1].ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        // GeoJSON coordinates are ordered [longitude, latitude].
        var parsedLongitude = point[0].GetDouble();
        var parsedLatitude = point[1].GetDouble();

        if (parsedLongitude is < -180 or > 180 || parsedLatitude is < -90 or > 90)
        {
            return false;
        }

        longitude = parsedLongitude;
        latitude = parsedLatitude;
        return true;
    }

    internal static bool TryParseSourceTimestamp(string? value, out DateTimeOffset timestamp)
    {
        // Timestamps without an explicit offset are treated as UTC, based on the observed API format.
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out timestamp);
    }
}
