using Domain.DTO;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace RadioSignalsWeb.Controllers;

[ApiController]
[Route("api/predict")] // Stable v1 prediction endpoint
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _svc;
    public PredictionsController(IPredictionService svc) => _svc = svc;

    [HttpPost]
    public async Task<ActionResult<PredictionResponseDto>> Predict([FromBody] PredictionDto dto)
    {
        // Minimal validation mirroring MeasurementDto rules
        if (dto.Technology == Technology.DIGITAL_TV && dto.ChannelNumber is null)
            return BadRequest("ChannelNumber is required for DIGITAL_TV");
        if (dto.Technology == Technology.FM && dto.FrequencyMHz is null)
            return BadRequest("FrequencyMHz is required for FM");

        try
        {
            var result = await _svc.PredictAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Bubble up Python service errors cleanly
            return StatusCode(502, ex.Message);
        }
    }
}

