using GrandPrix.Api.Models;

namespace GrandPrix.Api.DTOs;

/// <summary>
/// A registered vessel with its latest trusted observation. Measurements are null when no valid,
/// non-unavailable observation exists. Rank is set only for Fresh vessels.
/// </summary>
public sealed record ShipDto(
    string Name,
    long Mmsi,
    long Imo,
    double? SpeedKnots,
    double? Latitude,
    double? Longitude,
    double? CourseOverGround,
    int? TrueHeading,
    DateTimeOffset? SourceTimestampUtc,
    long? DataAgeSeconds,
    FreshnessStatus Status,
    int? Rank);
