using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class SettlementDto
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Settlement name is required.")]
    [StringLength(100, ErrorMessage = "Settlement name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Municipality ID is required.")]
    public Guid MunicipalityId { get; set; }

    public string? RegistryNumber { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Population must be a non-negative number.")]
    public int? Population { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Households must be a non-negative number.")]
    public int? Households { get; set; }

    public SettlementDto(Guid id, string name, int? population)
    {
        Id = id;
        Name = name;
        Population = population;
    }

    public SettlementDto()
    {
    }
}