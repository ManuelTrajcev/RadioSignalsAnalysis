using Domain.Enums;

namespace Domain.DTO;

public class MeasurementResponseDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string TestLocation { get; set; }

    public int LatitudeDegrees { get; set; }
    public int LatitudeMinutes { get; set; }
    public float LatitudeSeconds { get; set; }
    public int LongitudeDegrees { get; set; }
    public int LongitudeMinutes { get; set; }
    public float LongitudeSeconds { get; set; }

    public double LatitudeDecimal { get; set; }
    public double LongitudeDecimal { get; set; }
    public int AltitudeMeters { get; set; }

    public bool IsTvChannel { get; set; }
    public int? ChannelNumber { get; set; }
    public float? FrequencyMHz { get; set; }

    public string? ProgramIdentifier { get; set; }
    public string TransmitterLocation { get; set; }
    public float ElectricFieldDbuvPerM { get; set; }
    public string? Remarks { get; set; }
    public MeasurementStatus Status { get; set; }
    public Technology Technology { get; set; }

    // basics to help the UI
    public Guid SettlementId { get; set; }
    public string SettlementName { get; set; }
    public Guid MunicipalityId { get; set; }
    public string MunicipalityName { get; set; }
}
