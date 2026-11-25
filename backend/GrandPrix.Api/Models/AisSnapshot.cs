namespace GrandPrix.Api.Models;

public enum UpstreamConnectionStatus
{
    Unknown,
    Connected,
    Failed,
}

public sealed record AisSnapshot(
    IReadOnlyList<VesselObservation> Observations,
    DateTimeOffset? LastSuccessfulFetchUtc,
    DateTimeOffset? LastAttemptedFetchUtc,
    int FeaturesReceived,
    int MatchedVesselCount,
    int IdentityMismatchCount,
    UpstreamConnectionStatus UpstreamStatus,
    IReadOnlyList<string> ProcessingIssues,
    string? LastError);
