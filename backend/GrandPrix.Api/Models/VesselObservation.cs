using GrandPrix.Api.Models.Vessels;

namespace GrandPrix.Api.Models;

// SpeedKnots is stored as the raw upstream value. The real-time feed's unit is not yet
// confirmed by Kystverket; it is assumed to be knots, following the AIS standard.
public sealed record VesselObservation(
    RegisteredVessel Vessel,
    double SpeedKnots,
    double Latitude,
    double Longitude,
    double? CourseOverGround,
    int? TrueHeading,
    DateTimeOffset SourceTimestampUtc,
    DateTimeOffset RetrievedAtUtc);
