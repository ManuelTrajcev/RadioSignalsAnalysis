using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.DTO;

public class ThresholdDto
{
    [Required] public Technology Technology { get; set; }
    [Required] public Scope Scope { get; set; }
    public string? ScopeIdentifier { get; set; } // MunicipalityId/SettlementId/Transmitter text

    public int? ChannelNumber { get; set; }      // for DIGITAL_TV
    public float? FrequencyMHz { get; set; }     // for FM

    [Required] public float MinDbuVPerM { get; set; }
    [Required] public float MaxDbuVPerM { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
