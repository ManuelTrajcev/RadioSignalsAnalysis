using System.ComponentModel.DataAnnotations;

namespace Domain.Domain_Models;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}