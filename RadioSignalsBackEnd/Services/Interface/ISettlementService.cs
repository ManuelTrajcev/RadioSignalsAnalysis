using Domain.Domain_Models;
using Domain.DTO;

namespace Services.Interface;

public interface ISettlementService
{
    Task<Settlement> CreateAsync(SettlementDto createDto);
    Task<Settlement?> GetByIdAsync(Guid id);
}