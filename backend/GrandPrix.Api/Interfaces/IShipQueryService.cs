using GrandPrix.Api.DTOs;

namespace GrandPrix.Api.Interfaces;

public interface IShipQueryService
{
    IReadOnlyList<ShipDto> GetShips(DateTimeOffset nowUtc);

    ShipStatusDto GetStatus(DateTimeOffset nowUtc);
}
