using Domain.Domain_Models;
using Domain.DTO;

namespace Services.Interface;

public interface IThresholdService
{
    Task<IEnumerable<ReferenceThreshold>> GetAllAsync();
    Task<ReferenceThreshold?> GetByIdAsync(Guid id);
    Task<ReferenceThreshold> CreateAsync(ThresholdDto dto);
    Task<ReferenceThreshold?> UpdateAsync(Guid id, ThresholdDto dto);
    Task<bool> DeleteAsync(Guid id);
}
