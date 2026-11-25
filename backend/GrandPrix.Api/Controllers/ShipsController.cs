using GrandPrix.Api.DTOs;
using GrandPrix.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrandPrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipsController(IShipQueryService shipQueryService, TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<ShipDto>> GetShips()
    {
        return Ok(shipQueryService.GetShips(timeProvider.GetUtcNow()));
    }

    [HttpGet("status")]
    public ActionResult<ShipStatusDto> GetStatus()
    {
        return Ok(shipQueryService.GetStatus(timeProvider.GetUtcNow()));
    }
}
