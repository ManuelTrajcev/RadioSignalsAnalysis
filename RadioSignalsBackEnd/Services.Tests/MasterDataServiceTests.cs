using System.Linq.Expressions;
using Domain.Domain_Models;
using Domain.DTO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Repository.Interface;
using Services.Implementation;
using Services.Tests.TestHelpers;
using Xunit;

namespace Services.Tests;

public class MasterDataServiceTests
{
    private readonly Mock<IRepository<Municipality>> _municipalities = new();
    private readonly Mock<IRepository<Settlement>> _settlements = new();

    private MasterDataService CreateSut() => new(_municipalities.Object, _settlements.Object);

    [Fact]
    public async Task GetMunicipalitiesAsync_ReturnsRepositoryResult()
    {
        var options = new List<OptionDto> { new(Guid.NewGuid(), "Skopje"), new(Guid.NewGuid(), "Bitola") };
        _municipalities.SetupGetAll<Municipality, OptionDto>(options);

        var result = await CreateSut().GetMunicipalitiesAsync();

        result.Should().BeEquivalentTo(options);
    }

    [Fact]
    public async Task GetSettlementsAsync_ReturnsRepositoryResultAndQueriesSettlements()
    {
        var municipalityId = Guid.NewGuid();
        var dtos = new List<SettlementDto>
        {
            new(Guid.NewGuid(), "Centar", 5000),
            new(Guid.NewGuid(), "Karpos", 7000)
        };
        _settlements.SetupGetAll<Settlement, SettlementDto>(dtos);

        var result = await CreateSut().GetSettlementsAsync(municipalityId);

        result.Should().BeEquivalentTo(dtos);
        // The municipality filter is passed as a predicate to the repository.
        _settlements.Verify(r => r.GetAllAsync(
            It.IsAny<Expression<Func<Settlement, SettlementDto>>>(),
            It.IsAny<Expression<Func<Settlement, bool>>>(),
            It.IsAny<Func<IQueryable<Settlement>, IOrderedQueryable<Settlement>>>(),
            It.IsAny<Func<IQueryable<Settlement>, IIncludableQueryable<Settlement, object>>>()),
            Times.Once);
    }
}
