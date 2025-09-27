using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.DTO;

// Data contract for prediction requests. Mirrors MeasurementDto but omits
// ElectricFieldDbuvPerM, Status, and Remarks.
public class PredictionDto
{
    [Required] public Guid SettlementId { get; set; }
    [Required] public DateTime Date { get; set; }

    // Coordinates in DMS, same semantics as MeasurementDto
    [Required] public int LatitudeDegrees { get; set; }
    [Required] public int LatitudeMinutes { get; set; }
    [Required] public float LatitudeSeconds { get; set; }
    [Required] public int LongitudeDegrees { get; set; }
    [Required] public int LongitudeMinutes { get; set; }
    [Required] public float LongitudeSeconds { get; set; }

    [Required] public int AltitudeMeters { get; set; }

    // TV or FM (exactly one filled depending on Technology)
    public int? ChannelNumber { get; set; }
    public float? FrequencyMHz { get; set; }

    public string? ProgramIdentifier { get; set; }
    [Required, MaxLength(255)] public string TransmitterLocation { get; set; }

    [Required] public Technology Technology { get; set; }
}

