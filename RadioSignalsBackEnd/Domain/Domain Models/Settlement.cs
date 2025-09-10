using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Domain_Models;

public class Settlement : BaseEntity
{
    public string Name { get; set; }
    public string? RegistryNumber { get; set; }
    public int? Population { get; set; }
    public int? Households { get; set; }
    public Guid MunicipalityId { get; set; }

    [ForeignKey("MunicipalityId")] public Municipality? Municipality { get; set; }
}