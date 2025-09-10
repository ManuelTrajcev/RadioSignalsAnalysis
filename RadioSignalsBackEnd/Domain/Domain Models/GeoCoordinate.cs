using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domain_Models;

public class GeoCoordinate : BaseEntity
{
    [Required]
    public int LatitudeDegrees { get; set; }
    [Required]
    public int LatitudeMinutes { get; set; }
    [Required]
    public float LatitudeSeconds { get; set; }
    
    [Required]
    public int LongitudeDegrees { get; set; }
    [Required]
    public int LongitudeMinutes { get; set; }
    [Required]
    public float LongitudeSeconds { get; set; }

    // Persisted decimal values; will be set by MeasurementService on create/update
    public double LatitudeDecimal { get; set; }
    public double LongitudeDecimal { get; set; }

}