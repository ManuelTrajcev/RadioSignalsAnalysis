using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.DTO;

public class PredictionRequestDto
{
    [Required] public Guid SettlementId { get; set; }
    [Required] public DateTime Date { get; set; }

    // Raw coordinates in DMS as provided by the UI
    [Required] public int LatitudeDegrees { get; set; }
    [Required] public int LatitudeMinutes { get; set; }
    [Required] public float LatitudeSeconds { get; set; }
    [Required] public int LongitudeDegrees { get; set; }
    [Required] public int LongitudeMinutes { get; set; }
    [Required] public float LongitudeSeconds { get; set; }

    [Required] public int AltitudeMeters { get; set; }

    public int? ChannelNumber { get; set; }
    public float? FrequencyMHz { get; set; }

    public string? ProgramIdentifier { get; set; }
    public string? TransmitterLocation { get; set; }

    [Required] public Technology Technology { get; set; }
}
