using GrandPrix.Api.Models.Ais;

namespace GrandPrix.Api.Interfaces;

public interface IAisClient
{
    Task<AisFeatureCollection> GetRealtimeFeaturesAsync(CancellationToken cancellationToken);
}
