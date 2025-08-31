using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Domain_Models;

[Index(nameof(RegistryNumber), IsUnique = true)]
public class Settlement : BaseEntity
{
    [Required] public string Name { get; set; }
    [Required] public string RegistryNumber { get; set; }
    public int Population { get; set; }
    public int Households { get; set; }
    [Required] public int MunicipalityId { get; set; }
    [Required] public Municipality Municipality { get; set; }

    public Settlement(string name, string registryNumber, int population, int households, int municipalityId, Municipality municipality)
    {
        Name = name;
        RegistryNumber = registryNumber;
        Population = population;
        Households = households;
        MunicipalityId = municipalityId;
        Municipality = municipality;
    }
}