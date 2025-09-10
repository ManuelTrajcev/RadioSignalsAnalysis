using Domain.DTO;

namespace Services.Interface;

public interface IMasterDataService
{
    Task<IEnumerable<OptionDto>> GetMunicipalitiesAsync();
    Task<IEnumerable<OptionDto>> GetSettlementsAsync(Guid municipalityId);
}
