using Domain.Domain_Models;
using Domain.DTO;
using Repository.Interface;
using Services.Interface;

namespace Services.Implementation;

public class ThresholdService : IThresholdService
{
    private readonly IRepository<ReferenceThreshold> _repo;

    public ThresholdService(IRepository<ReferenceThreshold> repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ReferenceThreshold>> GetAllAsync()
        => await _repo.GetAllAsync(t => t, orderBy: q => q.OrderByDescending(x => x.CreatedAtUtc));

    public async Task<ReferenceThreshold?> GetByIdAsync(Guid id)
        => await _repo.GetAsync(t => t, t => t.Id == id);

    public async Task<ReferenceThreshold> CreateAsync(ThresholdDto dto)
    {
        var entity = Map(dto, new ReferenceThreshold());
        return await _repo.InsertAsync(entity);
    }

    public async Task<ReferenceThreshold?> UpdateAsync(Guid id, ThresholdDto dto)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;
        var mapped = Map(dto, existing);
        return await _repo.UpdateAsync(mapped);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return false;
        await _repo.DeleteAsync(existing, userId);
        return true;
    }

    private static ReferenceThreshold Map(ThresholdDto dto, ReferenceThreshold e)
    {
        e.Technology = dto.Technology;
        e.Scope = dto.Scope;
        e.ScopeIdentifier = dto.ScopeIdentifier;
        e.ChannelNumber = dto.ChannelNumber;
        e.FrequencyMHz = dto.FrequencyMHz;
        e.MinDbuVPerM = dto.MinDbuVPerM;
        e.MaxDbuVPerM = dto.MaxDbuVPerM;
        e.IsActive = dto.IsActive;
        e.Notes = dto.Notes;
        return e;
    }
}
