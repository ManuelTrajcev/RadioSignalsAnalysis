using Domain.DTO;
using Domain.Domain_Models;
using Repository.Interface;
using Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Services.Implementation;

public class MasterDataService : IMasterDataService
{
    private readonly IRepository<Municipality> _municipalities;
    private readonly IRepository<Settlement> _settlements;

    public MasterDataService(IRepository<Municipality> municipalities, IRepository<Settlement> settlements)
    {
        _municipalities = municipalities;
        _settlements = settlements;
    }

    public async Task<IEnumerable<OptionDto>> GetMunicipalitiesAsync()
        => await _municipalities.GetAllAsync(
            m => new OptionDto(m.Id, m.Name),
            orderBy: q => q.OrderBy(x => x.Name));

    public async Task<IEnumerable<SettlementDto>> GetSettlementsAsync(Guid municipalityId)
        => await _settlements.GetAllAsync(
            s => new SettlementDto(s.Id, s.Name, s.Population),
            s => s.MunicipalityId == municipalityId,
            q => q.OrderBy(x => x.Name));
}
