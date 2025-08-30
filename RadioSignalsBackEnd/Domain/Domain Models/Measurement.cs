using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Domain_Models;

public class Measurement : BaseEntity
{
    [Required]
    public int SettlementId { get; set; }
    public Settlement Settlement { get; set; }

    [Required]
    public DateTime Date { get; set; }

    // Потесна локација (стринг)
    [Required, MaxLength(255)]
    public string TestLocation { get; set; }

    // Координати (FK кон Coordinate)
    [Required]
    public int CoordinateId { get; set; }
    public GeoCoordinate Coordinate { get; set; }

    // Надморска височина (цел број)
    [Required]
    public int AltitudeMeters { get; set; }

    // Канал/фрекфенција (FK кон ChannelFrequency)
    [Required]
    public int ChannelFrequencyId { get; set; }
    public ChannelFrequency ChannelFrequency { get; set; }

    // Програма – идентификатор (опционално)
    [MaxLength(120)]
    public string ProgramIdentifier { get; set; }

    // Објект од каде се емитира (стринг)
    [Required, MaxLength(255)]
    public string TransmitterLocation { get; set; }

    // Електрично поле (цел број, dBµV/m)
    [Required]
    public ElectricFieldStrength ElectricFieldStrength { get; set; }
    
    public string Remarks { get; set; }

    public MeasurementStatus Status { get; set; }
}