namespace GrandPrix.Api.DTOs;

/// <summary>
/// Application and AIS feed status. Status counts cover all registered vessels.
/// </summary>
public sealed record ShipStatusDto(
    bool UpstreamReachable,
    string UpstreamStatus,
    DateTimeOffset? LastSuccessfulFetchUtc,
    DateTimeOffset? LastAttemptedFetchUtc,
    int PollingIntervalSeconds,
    int FeaturesReceived,
    int MatchedVesselCount,
    int VesselsDetected,
    int IdentityMismatchCount,
    int FreshCount,
    int DelayedCount,
    int StaleCount,
    int UnavailableCount,
    string? LastError);
