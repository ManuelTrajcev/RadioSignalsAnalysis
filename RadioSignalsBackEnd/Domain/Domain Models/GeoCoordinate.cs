using System.ComponentModel.DataAnnotations;

namespace Domain.Domain_Models;

public class GeoCoordinate
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

    public GeoCoordinate(int latitudeDegrees, int latitudeMinutes, float latitudeSeconds, int longitudeDegrees, int longitudeMinutes, float longitudeSeconds)
    {
        LatitudeDegrees = latitudeDegrees;
        LatitudeMinutes = latitudeMinutes;
        LatitudeSeconds = latitudeSeconds;
        LongitudeDegrees = longitudeDegrees;
        LongitudeMinutes = longitudeMinutes;
        LongitudeSeconds = longitudeSeconds;
    }
}