using Domain.DTO;

namespace Services.Interface;

public interface IPredictionService
{
    Task<PredictionResponseDto> PredictAsync(PredictionDto dto);
}

