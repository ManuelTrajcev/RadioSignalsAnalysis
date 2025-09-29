namespace Domain.Helpers;

public static class CoordinateHelper
{
    public static double ToDecimal(int degrees, int minutes, float seconds)
        => degrees + (minutes / 60.0) + (seconds / 3600.0);
}
