using Domain.Domain_Models;
using Domain.DTO;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Repository.Interface;
using Services.Implementation;
using Services.Tests.TestHelpers;
using Xunit;

namespace Services.Tests;

public class MeasurementServiceTests
{
    private readonly Mock<IRepository<Measurement>> _measurements = new();
    private readonly Mock<IRepository<GeoCoordinate>> _coordinates = new();
    private readonly Mock<IRepository<ChannelFrequency>> _frequencies = new();
    private readonly Mock<IRepository<ElectricFieldStrength>> _efs = new();
    private readonly Mock<IRepository<Settlement>> _settlements = new();

    private MeasurementService CreateSut() =>
        new(_measurements.Object, _coordinates.Object, _frequencies.Object, _efs.Object, _settlements.Object);

    private static MeasurementDto TvDto() => new()
    {
        SettlementId = Guid.NewGuid(),
        Date = new DateTime(2026, 5, 1),
        TestLocation = "Test loc",
        LatitudeDegrees = 41,
        LatitudeMinutes = 59,
        LatitudeSeconds = 30f,
        LongitudeDegrees = 21,
        LongitudeMinutes = 25,
        LongitudeSeconds = 45f,
        AltitudeMeters = 300,
        Population = null,
        ChannelNumber = 30,
        FrequencyMHz = null,
        ProgramIdentifier = "MKTV1",
        TransmitterLocation = "Vodno",
        ElectricFieldDbuvPerM = 55f,
        Remarks = "ok",
        Status = MeasurementStatus.Covered,
        Technology = Technology.DIGITAL_TV
    };

    /// <summary>Wires every repository for a successful CreateAsync; tests override individual seams.</summary>
    private void WireCreateDefaults(Settlement settlement)
    {
        _settlements.SetupGet<Settlement, Settlement>(settlement);   // EnsureSettlementExists
        _settlements.SetupUpdateEcho();
        _measurements.SetupGet<Measurement, Guid>(Guid.Empty);        // ExistsDuplicateAsync -> no duplicate
        _measurements.SetupInsertEcho();
        _coordinates.SetupGet<GeoCoordinate, GeoCoordinate>(null!);   // FindOrCreate -> create
        _coordinates.SetupInsertEcho();
        _coordinates.SetupUpdateEcho();
        _frequencies.SetupGet<ChannelFrequency, ChannelFrequency>(null!);
        _frequencies.SetupInsertEcho();
        _efs.SetupInsertEcho();
    }

    [Fact]
    public async Task CreateAsync_WhenSettlementMissing_Throws()
    {
        _settlements.SetupGet<Settlement, Settlement>(null!);

        var act = () => CreateSut().CreateAsync(TvDto());

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Settlement not found*");
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicate_Throws()
    {
        _settlements.SetupGet<Settlement, Settlement>(new Settlement { Population = 100 });
        _measurements.SetupGet<Measurement, Guid>(Guid.NewGuid()); // existing id => duplicate

        var act = () => CreateSut().CreateAsync(TvDto());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        _measurements.Verify(r => r.InsertAsync(It.IsAny<Measurement>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenFmFrequencyOutOfRange_Throws()
    {
        WireCreateDefaults(new Settlement { Population = 100 });
        var dto = TvDto();
        dto.Technology = Technology.FM;
        dto.ChannelNumber = null;
        dto.FrequencyMHz = 50.0f; // below 87.0

        var act = () => CreateSut().CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*between 87.0 and 107.9*");
    }

    [Fact]
    public async Task CreateAsync_WhenChannelOutOfRange_Throws()
    {
        WireCreateDefaults(new Settlement { Population = 100 });
        var dto = TvDto();
        dto.ChannelNumber = 10; // below 21

        var act = () => CreateSut().CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*between 21 and 65*");
    }

    [Fact]
    public async Task CreateAsync_HappyPath_BuildsMeasurementWiresRelationsAndInserts()
    {
        var settlement = new Settlement { Id = Guid.NewGuid(), Population = 5000 };
        WireCreateDefaults(settlement);

        var coord = new GeoCoordinate { Id = Guid.NewGuid() };
        var freq = new ChannelFrequency { Id = Guid.NewGuid(), IsTvChannel = true, ChannelNumber = 30 };
        var efs = new ElectricFieldStrength { Id = Guid.NewGuid() };
        _coordinates.Setup(r => r.InsertAsync(It.IsAny<GeoCoordinate>())).ReturnsAsync(coord);
        _frequencies.Setup(r => r.InsertAsync(It.IsAny<ChannelFrequency>())).ReturnsAsync(freq);
        _efs.Setup(r => r.InsertAsync(It.IsAny<ElectricFieldStrength>())).ReturnsAsync(efs);

        Measurement? inserted = null;
        _measurements.Setup(r => r.InsertAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => inserted = m)
            .ReturnsAsync((Measurement m) => m);

        var dto = TvDto();
        var result = await CreateSut().CreateAsync(dto);

        inserted.Should().NotBeNull();
        result.Should().BeSameAs(inserted);
        inserted!.SettlementId.Should().Be(dto.SettlementId);
        inserted.Technology.Should().Be(Technology.DIGITAL_TV);
        inserted.CoordinateId.Should().Be(coord.Id);
        inserted.ChannelFrequencyId.Should().Be(freq.Id);
        inserted.ElectricFieldStrengthId.Should().Be(efs.Id);
        inserted.TransmitterLocation.Should().Be("Vodno");
        inserted.CurrentPopulation.Should().Be(5000);
    }

    [Fact]
    public async Task CreateAsync_WhenPopulationProvided_UpdatesSettlement()
    {
        var settlement = new Settlement { Id = Guid.NewGuid(), Population = 100 };
        WireCreateDefaults(settlement);
        var dto = TvDto();
        dto.Population = 999;

        await CreateSut().CreateAsync(dto);

        settlement.Population.Should().Be(999);
        _settlements.Verify(r => r.UpdateAsync(settlement), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DigitalTvWithoutChannel_Throws()
    {
        WireCreateDefaults(new Settlement { Population = 100 });
        var dto = TvDto();
        dto.ChannelNumber = null;

        var act = () => CreateSut().CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ChannelNumber is required*");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsEntity()
    {
        var entity = new Measurement { Id = Guid.NewGuid() };
        _measurements.SetupGet<Measurement, Measurement>(entity);

        var result = await CreateSut().GetByIdAsync(entity.Id);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        _measurements.SetupGet<Measurement, Measurement>(null!);

        var result = await CreateSut().GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_ReturnsRepositoryResult()
    {
        var items = new List<Measurement> { new(), new() };
        _measurements.SetupGetAll<Measurement, Measurement>(items);

        var result = await CreateSut().QueryAsync(null, null, null, null, null);

        result.Should().BeEquivalentTo(items);
        _measurements.Verify(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Measurement, Measurement>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Measurement, bool>>>(),
            It.IsAny<Func<IQueryable<Measurement>, IOrderedQueryable<Measurement>>>(),
            It.IsAny<Func<IQueryable<Measurement>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Measurement, object>>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNull()
    {
        _measurements.SetupGet<Measurement, Measurement>(null!);

        var result = await CreateSut().UpdateAsync(Guid.NewGuid(), TvDto(), Guid.NewGuid());

        result.Should().BeNull();
        _measurements.Verify(r => r.UpdateAsync(It.IsAny<Measurement>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesFieldsAndPersists()
    {
        var existing = new Measurement
        {
            Id = Guid.NewGuid(),
            ElectricFieldStrengthId = Guid.NewGuid(),
            ElectricFieldStrength = new ElectricFieldStrength { Id = Guid.NewGuid(), Value = 1f }
        };
        _measurements.SetupGet<Measurement, Measurement>(existing);
        _measurements.SetupUpdateEcho();
        _settlements.SetupGet<Settlement, Settlement>(new Settlement { Id = Guid.NewGuid() });
        _coordinates.SetupGet<GeoCoordinate, GeoCoordinate>(null!);
        _coordinates.SetupInsertEcho();
        _frequencies.SetupGet<ChannelFrequency, ChannelFrequency>(null!);
        _frequencies.SetupInsertEcho();
        _efs.SetupUpdateEcho();

        var dto = TvDto();
        var userId = Guid.NewGuid();
        var result = await CreateSut().UpdateAsync(existing.Id, dto, userId);

        result.Should().BeSameAs(existing);
        existing.SettlementId.Should().Be(dto.SettlementId);
        existing.TransmitterLocation.Should().Be("Vodno");
        existing.Technology.Should().Be(Technology.DIGITAL_TV);
        existing.LastEditedBy.Should().Be(userId);
        existing.ElectricFieldStrength!.Value.Should().Be(dto.ElectricFieldDbuvPerM);
        _measurements.Verify(r => r.UpdateAsync(existing), Times.Once);
        _efs.Verify(r => r.UpdateAsync(existing.ElectricFieldStrength), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        _measurements.SetupGet<Measurement, Measurement>(null!);

        var result = await CreateSut().DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
        _measurements.Verify(r => r.DeleteAsync(It.IsAny<Measurement>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var existing = new Measurement { Id = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        _measurements.SetupGet<Measurement, Measurement>(existing);
        _measurements.SetupDeleteEcho();

        var result = await CreateSut().DeleteAsync(existing.Id, userId);

        result.Should().BeTrue();
        _measurements.Verify(r => r.DeleteAsync(existing, userId), Times.Once);
    }
}
