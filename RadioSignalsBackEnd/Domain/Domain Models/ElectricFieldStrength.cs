using Domain.Enums;

namespace Domain.Domain_Models;

public class ElectricFieldStrength : BaseEntity
{
    public float Value { get; set; }
    public ElectricFieldUnit MesurementUnit { get; set; }

    public ElectricFieldStrength(float value, ElectricFieldUnit mesurementUnit)
    {
        Value = value;
        MesurementUnit = mesurementUnit;
    }
}