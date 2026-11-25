using GrandPrix.Api.Configuration;
using GrandPrix.Api.DTOs;
using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Models;
using GrandPrix.Api.Models.Vessels;
using Microsoft.Extensions.Options;

namespace GrandPrix.Api.Services;

public sealed class ShipQueryService(
    IAisSnapshotStore snapshotStore,
    IOptions<AisSettings> settings) : IShipQueryService
{
    public IReadOnlyList<ShipDto> GetShips(DateTimeOffset nowUtc)
    {
        var snapshot = snapshotStore.GetSnapshot();
        var observationsByMmsi = snapshot.Observations.ToDictionary(o => o.Vessel.Mmsi);

        var ships = VesselRegistry.Vessels
            .Select(vessel => BuildShip(vessel, observationsByMmsi.GetValueOrDefault(vessel.Mmsi), nowUtc))
            .ToList();

        // Delayed vessels keep their place using their last known speed until a faster vessel overtakes them
        // or their data becomes stale.
        var ranked = ships
            .Where(s => s.Status is FreshnessStatus.Fresh or FreshnessStatus.Delayed)
            .OrderByDescending(s => s.SpeedKnots)
            .ThenBy(s => s.Name, StringComparer.Ordinal)
            .Select((ship, index) => ship with { Rank = index + 1 });

        var unranked = ships
            .Where(s => s.Status is FreshnessStatus.Stale or FreshnessStatus.Unavailable)
            .OrderBy(s => s.Status)
            .ThenBy(s => s.Name, StringComparer.Ordinal);

        return ranked.Concat(unranked).ToList();
    }

    public ShipStatusDto GetStatus(DateTimeOffset nowUtc)
    {
        var snapshot = snapshotStore.GetSnapshot();
        var ships = GetShips(nowUtc);

        return new ShipStatusDto(
            UpstreamReachable: snapshot.UpstreamStatus == UpstreamConnectionStatus.Connected,
            UpstreamStatus: snapshot.UpstreamStatus.ToString(),
            LastSuccessfulFetchUtc: snapshot.LastSuccessfulFetchUtc,
            LastAttemptedFetchUtc: snapshot.LastAttemptedFetchUtc,
            PollingIntervalSeconds: settings.Value.PollingIntervalSeconds,
            FeaturesReceived: snapshot.FeaturesReceived,
            MatchedVesselCount: snapshot.MatchedVesselCount,
            VesselsDetected: ships.Count(s => s.SourceTimestampUtc is not null),
            IdentityMismatchCount: snapshot.IdentityMismatchCount,
            FreshCount: ships.Count(s => s.Status == FreshnessStatus.Fresh),
            DelayedCount: ships.Count(s => s.Status == FreshnessStatus.Delayed),
            StaleCount: ships.Count(s => s.Status == FreshnessStatus.Stale),
            UnavailableCount: ships.Count(s => s.Status == FreshnessStatus.Unavailable),
            LastError: snapshot.LastError);
    }

    private ShipDto BuildShip(RegisteredVessel vessel, VesselObservation? observation, DateTimeOffset nowUtc)
    {
        if (observation is null)
        {
            return new ShipDto(
                vessel.Name, vessel.Mmsi, vessel.Imo,
                SpeedKnots: null, Latitude: null, Longitude: null,
                CourseOverGround: null, TrueHeading: null,
                SourceTimestampUtc: null, DataAgeSeconds: null,
                Status: FreshnessStatus.Unavailable, Rank: null);
        }

        var ageSeconds = FreshnessClassifier.CalculateAgeSeconds(observation.SourceTimestampUtc, nowUtc);
        var status = FreshnessClassifier.Classify(ageSeconds, settings.Value);

        // Unavailable observations never expose speed or position, to avoid presenting stale data as live.
        var hasMeasurements = status != FreshnessStatus.Unavailable;

        return new ShipDto(
            vessel.Name, vessel.Mmsi, vessel.Imo,
            SpeedKnots: hasMeasurements ? observation.SpeedKnots : null,
            Latitude: hasMeasurements ? observation.Latitude : null,
            Longitude: hasMeasurements ? observation.Longitude : null,
            CourseOverGround: hasMeasurements ? observation.CourseOverGround : null,
            TrueHeading: hasMeasurements ? observation.TrueHeading : null,
            SourceTimestampUtc: observation.SourceTimestampUtc,
            DataAgeSeconds: ageSeconds,
            Status: status,
            Rank: null);
    }
}
