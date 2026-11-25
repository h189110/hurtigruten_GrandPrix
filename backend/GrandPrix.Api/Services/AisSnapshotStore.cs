using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Models;

namespace GrandPrix.Api.Services;

public sealed class AisSnapshotStore : IAisSnapshotStore
{
    private readonly object _lock = new();
    private AisSnapshot _snapshot = new(
        Observations: [],
        LastSuccessfulFetchUtc: null,
        LastAttemptedFetchUtc: null,
        FeaturesReceived: 0,
        MatchedVesselCount: 0,
        IdentityMismatchCount: 0,
        UpstreamStatus: UpstreamConnectionStatus.Unknown,
        ProcessingIssues: [],
        LastError: null);

    public AisSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return _snapshot;
        }
    }

    public void RecordAttempt(DateTimeOffset attemptedAtUtc)
    {
        lock (_lock)
        {
            _snapshot = _snapshot with { LastAttemptedFetchUtc = attemptedAtUtc };
        }
    }

    public void RecordSuccess(AisProcessingResult result, DateTimeOffset fetchedAtUtc)
    {
        lock (_lock)
        {
            // Keep the newest known observation per vessel, so a vessel missing from one
            // successful response ages out naturally instead of disappearing immediately.
            var merged = new Dictionary<long, VesselObservation>();
            foreach (var observation in _snapshot.Observations.Concat(result.Observations))
            {
                var mmsi = observation.Vessel.Mmsi;
                if (!merged.TryGetValue(mmsi, out var existing) || existing.SourceTimestampUtc <= observation.SourceTimestampUtc)
                {
                    merged[mmsi] = observation;
                }
            }

            _snapshot = new AisSnapshot(
                Observations: merged.Values.ToList(),
                LastSuccessfulFetchUtc: fetchedAtUtc,
                LastAttemptedFetchUtc: fetchedAtUtc,
                FeaturesReceived: result.FeaturesReceived,
                MatchedVesselCount: result.MatchedVesselCount,
                IdentityMismatchCount: result.IdentityMismatchCount,
                UpstreamStatus: UpstreamConnectionStatus.Connected,
                ProcessingIssues: result.Issues,
                LastError: null);
        }
    }

    public void RecordFailure(string error)
    {
        lock (_lock)
        {
            // Previous observations and the last successful fetch time are intentionally kept.
            _snapshot = _snapshot with
            {
                UpstreamStatus = UpstreamConnectionStatus.Failed,
                LastError = error,
            };
        }
    }
}
