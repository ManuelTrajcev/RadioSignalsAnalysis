using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Domain_Models;

public class ChannelFrequency : BaseEntity
{
    // TRUE => TV channel (use ChannelNumber).
    // FALSE => FM frequency (use FrequencyMHz).
    [Required]
    public bool IsTvChannel { get; set; }

    // TV channel number (nullable; required when IsTvChannel = true).
    public int? ChannelNumber { get; set; }

    // FM frequency in MHz (nullable; required when IsTvChannel = false).
    public float? FrequencyMHz { get; set; }

    public FrequencyUnit? FrequencyUnit { get; set; }
}