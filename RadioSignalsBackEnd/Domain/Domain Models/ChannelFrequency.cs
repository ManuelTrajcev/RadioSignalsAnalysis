using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Domain_Models;

public class ChannelFrequency : BaseEntity
{
    [Required]
    public float Value { get; set; }
    [Required]
    public SignalType SignalType { get; set; }
    [Required]
    public FrequencyUnit FrequencyUnit { get; set; }

    public ChannelFrequency(float value, SignalType signalType, FrequencyUnit frequencyUnit)
    {
        Value = value;
        SignalType = signalType;
        FrequencyUnit = frequencyUnit;
    }
}