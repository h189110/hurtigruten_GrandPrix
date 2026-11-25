using GrandPrix.Api.Models;
using GrandPrix.Api.Models.Ais;

namespace GrandPrix.Api.Interfaces;

public interface IAisProcessingService
{
    AisProcessingResult Process(AisFeatureCollection featureCollection, DateTimeOffset retrievedAtUtc);
}

public sealed record AisProcessingResult(
    int FeaturesReceived,
    int MatchedVesselCount,
    IReadOnlyList<VesselObservation> Observations,
    IReadOnlyList<string> Issues,
    int IdentityMismatchCount);
