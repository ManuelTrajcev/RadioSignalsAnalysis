using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace RadioSignalsWeb.Controllers;

[ApiController]
[Route("api/master-data")]
[AllowAnonymous] // safe, read-only
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _svc;
    public MasterDataController(IMasterDataService svc) => _svc = svc;

    [HttpGet("municipalities")]
    public async Task<ActionResult<IEnumerable<OptionDto>>> GetMunicipalities()
        => Ok(await _svc.GetMunicipalitiesAsync());

    [HttpGet("municipalities/{municipalityId:guid}/settlements")]
    public async Task<ActionResult<IEnumerable<OptionDto>>> GetSettlements([FromRoute] Guid municipalityId)
        => Ok(await _svc.GetSettlementsAsync(municipalityId));
}
