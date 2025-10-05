using Domain.Domain_Models;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Services.Interface;

namespace Services.Implementation;

public class SettlementService : ISettlementService
{
    private readonly IRepository<Settlement> _settlementRepository;

    public SettlementService(IRepository<Settlement> settlementRepository)
    {
        _settlementRepository = settlementRepository;
    }

    public async Task<Settlement> CreateAsync(SettlementDto createDto)
    {
        var newSettlement = new Settlement
        {
            Name = createDto.Name,
            MunicipalityId = createDto.MunicipalityId,
            RegistryNumber = createDto.RegistryNumber,
            Population = createDto.Population,
            Households = createDto.Households,
        };

        var createdSettlement = await _settlementRepository.InsertAsync(newSettlement);

        var settlementDto = new SettlementDto
        {
            Name = createdSettlement.Name,
            MunicipalityId = createdSettlement.MunicipalityId,
            RegistryNumber = createdSettlement.RegistryNumber,
            Population = createdSettlement.Population,
            Households = createdSettlement.Households,
        };

        return newSettlement;
    }

    public async Task<Settlement?> GetByIdAsync(Guid id)
    {
        return await _settlementRepository.GetAsync(
            selector: s => s,
            predicate: s => s.Id == id,
            include: q => q
                .Include(s => s.Municipality)! 
        );
    }
}