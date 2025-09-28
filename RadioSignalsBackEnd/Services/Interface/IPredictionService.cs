using Domain.DTO;

namespace Services.Interface;

public interface IPredictionService
{
    Task<PredictionResponseDto> PredictAsync(PredictionRequestDto dto);
}
