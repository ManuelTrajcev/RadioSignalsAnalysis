using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domain_Models;

public class ElectricFieldStrength : BaseEntity
{
    public float Value { get; set; }

    [Column("MesurementUnit")]
    public ElectricFieldUnit MeasurementUnit { get; set; }
}