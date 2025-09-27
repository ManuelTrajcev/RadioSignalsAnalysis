namespace Domain.DTO;

public class PredictionResponseDto
{
    // Predicted electric field in dBµV/m
    public float ElectricFieldDbuvPerM { get; set; }
    public string Technology { get; set; }
    public string Model { get; set; }
}

