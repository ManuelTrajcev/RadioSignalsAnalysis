using Domain.Domain_Models;
using Domain.DTO;
using FluentAssertions;
using Moq;
using Repository.Interface;
using Services.Implementation;
using Services.Tests.TestHelpers;
using Xunit;

namespace Services.Tests;

public class SettlementServiceTests
{
    private readonly Mock<IRepository<Settlement>> _repo = new();

    private SettlementService CreateSut() => new(_repo.Object);

    [Fact]
    public async Task CreateAsync_BuildsSettlementFromDtoAndInserts()
    {
        var dto = new SettlementDto
        {
            Name = "Bitola",
            MunicipalityId = Guid.NewGuid(),
            RegistryNumber = "RN-1",
            Population = 1200,
            Households = 400
        };
        Settlement? inserted = null;
        _repo.Setup(r => r.InsertAsync(It.IsAny<Settlement>()))
            .Callback<Settlement>(s => inserted = s)
            .ReturnsAsync((Settlement s) => s);

        var result = await CreateSut().CreateAsync(dto);

        inserted.Should().NotBeNull();
        result.Should().BeSameAs(inserted);
        inserted!.Name.Should().Be("Bitola");
        inserted.MunicipalityId.Should().Be(dto.MunicipalityId);
        inserted.RegistryNumber.Should().Be("RN-1");
        inserted.Population.Should().Be(1200);
        inserted.Households.Should().Be(400);
        _repo.Verify(r => r.InsertAsync(It.IsAny<Settlement>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsEntity()
    {
        var entity = new Settlement { Id = Guid.NewGuid(), Name = "Ohrid" };
        _repo.SetupGet<Settlement, Settlement>(entity);

        var result = await CreateSut().GetByIdAsync(entity.Id);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        _repo.SetupGet<Settlement, Settlement>(null!);

        var result = await CreateSut().GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
