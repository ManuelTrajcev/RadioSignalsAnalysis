using Domain.Domain_Models;
using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace WebApi.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class SettlementController : ControllerBase
{
    private readonly ISettlementService _settlementService;

    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    /// <summary>
    /// Creates a new settlement.
    /// </summary>
    /// <param name="settlementDto">The data transfer object containing the new settlement details.</param>
    /// <returns>A 201 Created response with the newly created settlement data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SettlementDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] SettlementDto settlementDto)
    {
        // Model validation is handled automatically by [ApiController] and DTO annotations.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _settlementService.CreateAsync(settlementDto);

            // Return a 201 Created response.
            // Note: If you implement a separate GetById method later, you should point 
            // CreatedAtAction to that method instead of 'Create'.
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
      
            return StatusCode(500, "An error occurred while creating the settlement.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var m = await _settlementService.GetByIdAsync(id);
        return m == null ? NotFound() : Ok(ToResponse(m));
    }

    private static SettlementResponseDto ToResponse(Settlement s)
    {
        return new SettlementResponseDto
        {
            Id = s.Id,
            Name = s.Name,
            MunicipalityId = s.MunicipalityId,
            RegistryNumber = s.RegistryNumber,
            Population = s.Population,
            Households = s.Households,
        };
    }
}