using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.DTO;

public class MeasurementDto
{
    [Required] public Guid SettlementId { get; set; }
    [Required] public DateTime Date { get; set; }
    [Required, MaxLength(255)] public string TestLocation { get; set; }

    // DMS input
    [Required] public int LatitudeDegrees { get; set; }
    [Required] public int LatitudeMinutes { get; set; }
    [Required] public float LatitudeSeconds { get; set; }
    [Required] public int LongitudeDegrees { get; set; }
    [Required] public int LongitudeMinutes { get; set; }
    [Required] public float LongitudeSeconds { get; set; }

    [Required] public int AltitudeMeters { get; set; }
    [Required] public int Population { get; set; }

    // TV or FM (exactly one filled)
    public int? ChannelNumber { get; set; }
    public float? FrequencyMHz { get; set; }

    public string? ProgramIdentifier { get; set; }
    [Required, MaxLength(255)] public string TransmitterLocation { get; set; }

    [Required] public float ElectricFieldDbuvPerM { get; set; }
    public string? Remarks { get; set; }

    [Required] public MeasurementStatus Status { get; set; }
    [Required] public Technology Technology { get; set; }
}
