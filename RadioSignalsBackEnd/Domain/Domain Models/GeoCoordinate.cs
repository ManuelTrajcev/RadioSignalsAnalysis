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

    [NotMapped]
    public double LatitudeDecimal =>
        LatitudeDegrees + (LatitudeMinutes / 60.0) + (LatitudeSeconds / 3600.0);

    [NotMapped]
    public double LongitudeDecimal =>
        LongitudeDegrees + (LongitudeMinutes / 60.0) + (LongitudeSeconds / 3600.0);

}