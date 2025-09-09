using System.ComponentModel.DataAnnotations;

namespace Domain.Domain_Models;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}