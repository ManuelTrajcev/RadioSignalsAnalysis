using Domain.Domain_Models;
using Domain.DTO;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Services.Interface;

namespace Services.Implementation;

public class MeasurementService : IMeasurementService
{
    private readonly IRepository<Measurement> _measurements;
    private readonly IRepository<GeoCoordinate> _coordinates;
    private readonly IRepository<ChannelFrequency> _frequencies;
    private readonly IRepository<ElectricFieldStrength> _efs;
    private readonly IRepository<Settlement> _settlements;

    public MeasurementService(
        IRepository<Measurement> measurements,
        IRepository<GeoCoordinate> coordinates,
        IRepository<ChannelFrequency> frequencies,
        IRepository<ElectricFieldStrength> efs,
        IRepository<Settlement> settlements)
    {
        _measurements = measurements;
        _coordinates = coordinates;
        _frequencies = frequencies;
        _efs = efs;
        _settlements = settlements;
    }

    public async Task<Measurement> CreateAsync(MeasurementDto dto)
    {
        await EnsureSettlementExists(dto.SettlementId);

        var coord = await FindOrCreateCoordinateAsync(dto);
        var freq = await FindOrCreateFrequencyAsync(dto);
        var e = await _efs.InsertAsync(new ElectricFieldStrength
        {
            Value = dto.ElectricFieldDbuvPerM,
            MeasurementUnit = ElectricFieldUnit.dBuVPerMeter
        });

        var entity = new Measurement
        {
            SettlementId = dto.SettlementId,
            Date = dto.Date,
            TestLocation = dto.TestLocation,
            CoordinateId = coord.Id,
            AltitudeMeters = dto.AltitudeMeters,
            ChannelFrequencyId = freq.Id,
            ProgramIdentifier = dto.ProgramIdentifier,
            TransmitterLocation = dto.TransmitterLocation,
            ElectricFieldStrengthId = e.Id,
            Remarks = dto.Remarks,
            Status = dto.Status,
            Technology = dto.Technology
        };
        return await _measurements.InsertAsync(entity);
    }

    public async Task<Measurement?> GetByIdAsync(Guid id)
        => await _measurements.GetAsync(
            selector: m => m,
            predicate: m => m.Id == id,
            include: q => q
                .Include(m => m.ChannelFrequency)
                .Include(m => m.Coordinate)
                .Include(m => m.Settlement)!.ThenInclude(s => s.Municipality)
                .Include(m => m.ElectricFieldStrength));

    public async Task<IEnumerable<Measurement>> QueryAsync(
        Guid? municipalityId, Guid? settlementId,
        DateTime? dateFrom, DateTime? dateTo, Technology? technology)
    {
        return await _measurements.GetAllAsync(
            selector: m => m,
            predicate: m =>
                (!municipalityId.HasValue || (m.Settlement != null && m.Settlement.MunicipalityId == municipalityId)) &&
                (!settlementId.HasValue || m.SettlementId == settlementId) &&
                (!dateFrom.HasValue || m.Date >= dateFrom.Value) &&
                (!dateTo.HasValue || m.Date <= dateTo.Value) &&
                (!technology.HasValue || m.Technology == technology.Value),
            include: q => q
                .Include(m => m.ChannelFrequency)
                .Include(m => m.Coordinate)
                .Include(m => m.Settlement)!.ThenInclude(s => s.Municipality)
                .Include(m => m.ElectricFieldStrength),
            orderBy: q => q.OrderByDescending(m => m.Date).ThenBy(m => m.Id));
    }

    public async Task<Measurement?> UpdateAsync(Guid id, MeasurementDto dto)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        await EnsureSettlementExists(dto.SettlementId);

        var coord = await FindOrCreateCoordinateAsync(dto);
        var freq = await FindOrCreateFrequencyAsync(dto);
        var e = existing.ElectricFieldStrengthId;

        // update E-field value in place (simple path)
        if (existing.ElectricFieldStrength == null)
        {
            var newE = await _efs.InsertAsync(new ElectricFieldStrength
            {
                Value = dto.ElectricFieldDbuvPerM,
                MeasurementUnit = ElectricFieldUnit.dBuVPerMeter
            });
            e = newE.Id;
        }
        else
        {
            existing.ElectricFieldStrength.Value = dto.ElectricFieldDbuvPerM;
            await _efs.UpdateAsync(existing.ElectricFieldStrength);
        }

        existing.SettlementId = dto.SettlementId;
        existing.Date = dto.Date;
        existing.TestLocation = dto.TestLocation;
        existing.CoordinateId = coord.Id;
        existing.AltitudeMeters = dto.AltitudeMeters;
        existing.ChannelFrequencyId = freq.Id;
        existing.ProgramIdentifier = dto.ProgramIdentifier;
        existing.TransmitterLocation = dto.TransmitterLocation;
        existing.ElectricFieldStrengthId = e;
        existing.Remarks = dto.Remarks;
        existing.Status = dto.Status;
        existing.Technology = dto.Technology;

        return await _measurements.UpdateAsync(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var m = await GetByIdAsync(id);
        if (m == null) return false;
        await _measurements.DeleteAsync(m);
        return true;
    }

    private async Task<GeoCoordinate> FindOrCreateCoordinateAsync(MeasurementDto dto)
    {
        var latD = CoordinateHelper.ToDecimal(dto.LatitudeDegrees, dto.LatitudeMinutes, dto.LatitudeSeconds);
        var lonD = CoordinateHelper.ToDecimal(dto.LongitudeDegrees, dto.LongitudeMinutes, dto.LongitudeSeconds);

        var existing = await _coordinates.GetAsync(
            c => c,
            c => c.LatitudeDegrees == dto.LatitudeDegrees &&
                 c.LatitudeMinutes == dto.LatitudeMinutes &&
                 Math.Abs(c.LatitudeSeconds - dto.LatitudeSeconds) < 0.0001 &&
                 c.LongitudeDegrees == dto.LongitudeDegrees &&
                 c.LongitudeMinutes == dto.LongitudeMinutes &&
                 Math.Abs(c.LongitudeSeconds - dto.LongitudeSeconds) < 0.0001);

        if (existing != null)
        {
            // keep decimals fresh in case formula changed
            existing.LatitudeDecimal = latD;
            existing.LongitudeDecimal = lonD;
            return await _coordinates.UpdateAsync(existing);
        }

        return await _coordinates.InsertAsync(new GeoCoordinate
        {
            LatitudeDegrees = dto.LatitudeDegrees,
            LatitudeMinutes = dto.LatitudeMinutes,
            LatitudeSeconds = dto.LatitudeSeconds,
            LongitudeDegrees = dto.LongitudeDegrees,
            LongitudeMinutes = dto.LongitudeMinutes,
            LongitudeSeconds = dto.LongitudeSeconds,
            LatitudeDecimal = latD,
            LongitudeDecimal = lonD
        });
    }

    private async Task<ChannelFrequency> FindOrCreateFrequencyAsync(MeasurementDto dto)
    {
        if (dto.Technology == Technology.DIGITAL_TV)
        {
            if (dto.ChannelNumber is null) throw new ArgumentException("ChannelNumber is required for DIGITAL_TV");
            var found = await _frequencies.GetAsync(
                f => f,
                f => f.IsTvChannel && f.ChannelNumber == dto.ChannelNumber);
            return found ?? await _frequencies.InsertAsync(new ChannelFrequency
            {
                IsTvChannel = true,
                ChannelNumber = dto.ChannelNumber,
                FrequencyMHz = null // check constraint
            });
        }
        else // FM
        {
            if (dto.FrequencyMHz is null) throw new ArgumentException("FrequencyMHz is required for FM");
            var found = await _frequencies.GetAsync(
                f => f,
                f => !f.IsTvChannel && Math.Abs(f.FrequencyMHz!.Value - dto.FrequencyMHz.Value) < 0.0001f);
            return found ?? await _frequencies.InsertAsync(new ChannelFrequency
            {
                IsTvChannel = false,
                ChannelNumber = null,
                FrequencyMHz = dto.FrequencyMHz
            });
        }
    }

    private async Task EnsureSettlementExists(Guid settlementId)
    {
        var s = await _settlements.GetAsync(x => x, x => x.Id == settlementId);
        if (s == null) throw new ArgumentException("Settlement not found.", nameof(settlementId));
    }
}
