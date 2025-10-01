using System.Security.Claims;
using Domain.DTO;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace RadioSignalsWeb.Controllers;

[ApiController]
[Route("api/measurements")]
[Authorize] // all endpoints require an authenticated user unless noted below
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementService _svc;

    public MeasurementsController(IMeasurementService svc)
    {
        _svc = svc;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeasurementDto dto)
    {
        try
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Query(
        [FromQuery] Guid? municipalityId,
        [FromQuery] Guid? settlementId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Technology? technology)
    {
        var list = await _svc.QueryAsync(municipalityId, settlementId, dateFrom, dateTo, technology);
        return Ok(list.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var m = await _svc.GetByIdAsync(id);
        return m == null ? NotFound() : Ok(ToResponse(m));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MeasurementDto dto)
    {
        try
        {
            var currentUser = HttpContext.User;
            var userId = currentUser.FindFirst("sub")?.Value;

            if (userId != null)
            {
                var userIdGuid = Guid.Parse(userId);

                var updated = await _svc.UpdateAsync(id, dto, userIdGuid);
                return updated == null ? NotFound() : Ok(ToResponse(updated));
            }
            else
            {
                return Forbid();
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = HttpContext.User;
        var userId = currentUser.FindFirst("sub")?.Value; 
            
        if (userId != null)
        {
            var userIdGuid = Guid.Parse(userId);
            var ok = await _svc.DeleteAsync(id, userIdGuid);
            return ok ? NoContent() : NotFound();
        }

        return Forbid();
    }

    private static MeasurementResponseDto ToResponse(Domain.Domain_Models.Measurement m)
    {
        return new MeasurementResponseDto
        {
            Id = m.Id,
            Date = m.Date,
            TestLocation = m.TestLocation,
            LatitudeDecimal = m.Coordinate?.LatitudeDecimal ?? 0,
            LongitudeDecimal = m.Coordinate?.LongitudeDecimal ?? 0,
            LatitudeDegrees = m.Coordinate?.LatitudeDegrees ?? 0,
            LatitudeMinutes = m.Coordinate?.LatitudeMinutes ?? 0,
            LatitudeSeconds = m.Coordinate?.LatitudeSeconds ?? 0,
            LongitudeDegrees = m.Coordinate?.LongitudeDegrees ?? 0,
            LongitudeMinutes = m.Coordinate?.LongitudeMinutes ?? 0,
            LongitudeSeconds = m.Coordinate?.LongitudeSeconds ?? 0,
            AltitudeMeters = m.AltitudeMeters,
            IsTvChannel = m.ChannelFrequency?.IsTvChannel ?? false,
            ChannelNumber = m.ChannelFrequency?.ChannelNumber,
            FrequencyMHz = m.ChannelFrequency?.FrequencyMHz,
            ProgramIdentifier = m.ProgramIdentifier,
            TransmitterLocation = m.TransmitterLocation,
            ElectricFieldDbuvPerM = m.ElectricFieldStrength?.Value ?? 0,
            Remarks = m.Remarks,
            Status = m.Status,
            Technology = m.Technology,
            SettlementId = m.SettlementId,
            Population = m.CurrentPopulation,
            SettlementName = m.Settlement?.Name ?? string.Empty,
            MunicipalityId = m.Settlement?.MunicipalityId ?? Guid.Empty,
            MunicipalityName = m.Settlement?.Municipality?.Name ?? string.Empty
        };
    }
}