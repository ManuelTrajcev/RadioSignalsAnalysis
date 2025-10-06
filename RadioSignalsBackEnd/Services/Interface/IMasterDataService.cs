using Domain.DTO;

namespace Services.Interface;

public interface IMasterDataService
{
    Task<IEnumerable<OptionDto>> GetMunicipalitiesAsync();
    Task<IEnumerable<SettlementDto>> GetSettlementsAsync(Guid municipalityId);
}
