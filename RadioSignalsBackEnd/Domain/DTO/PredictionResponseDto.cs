using System.Collections.Generic;
using Domain.Enums;

namespace Domain.DTO;

public class PredictionResponseDto
{
    public Technology Technology { get; set; }
    public double FieldDbuvPerM { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public Dictionary<string, object> Features { get; set; } = new();
}
