using Domain.DTO;
using Domain.Domain_Models;
using Domain.Enums;

namespace Services.Interface;

public interface IMeasurementService
{
    Task<Measurement> CreateAsync(MeasurementDto dto);
    Task<Measurement?> GetByIdAsync(Guid id);
    Task<IEnumerable<Measurement>> QueryAsync(
        Guid? municipalityId, Guid? settlementId,
        DateTime? dateFrom, DateTime? dateTo,
        Technology? technology);
    Task<Measurement?> UpdateAsync(Guid id, MeasurementDto dto, Guid userIdGuid);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
