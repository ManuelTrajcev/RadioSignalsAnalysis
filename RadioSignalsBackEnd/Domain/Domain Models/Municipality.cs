using System.ComponentModel.DataAnnotations;

namespace Domain.Domain_Models;

public class Municipality : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public Municipality(string name)
    {
        Name = name;
    }
}