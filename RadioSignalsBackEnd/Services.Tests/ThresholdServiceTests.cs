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

public class ThresholdServiceTests
{
    private readonly Mock<IRepository<ReferenceThreshold>> _repo = new();

    private ThresholdService CreateSut() => new(_repo.Object);

    private static ThresholdDto ValidDto() => new()
    {
        Technology = Technology.FM,
        Scope = Scope.Municipality,
        ScopeIdentifier = "skopje",
        ChannelNumber = null,
        FrequencyMHz = 99.5f,
        MinDbuVPerM = 30f,
        MaxDbuVPerM = 80f,
        IsActive = true,
        Notes = "note"
    };

    [Fact]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var items = new List<ReferenceThreshold> { new(), new() };
        _repo.SetupGetAll<ReferenceThreshold, ReferenceThreshold>(items);

        var result = await CreateSut().GetAllAsync();

        result.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsEntity()
    {
        var entity = new ReferenceThreshold { Id = Guid.NewGuid() };
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(entity);

        var result = await CreateSut().GetByIdAsync(entity.Id);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(null!);

        var result = await CreateSut().GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_MapsDtoOntoNewEntityAndInserts()
    {
        var dto = ValidDto();
        ReferenceThreshold? inserted = null;
        _repo.Setup(r => r.InsertAsync(It.IsAny<ReferenceThreshold>()))
            .Callback<ReferenceThreshold>(e => inserted = e)
            .ReturnsAsync((ReferenceThreshold e) => e);

        var result = await CreateSut().CreateAsync(dto);

        inserted.Should().NotBeNull();
        result.Should().BeSameAs(inserted);
        inserted!.Technology.Should().Be(dto.Technology);
        inserted.Scope.Should().Be(dto.Scope);
        inserted.ScopeIdentifier.Should().Be(dto.ScopeIdentifier);
        inserted.ChannelNumber.Should().Be(dto.ChannelNumber);
        inserted.FrequencyMHz.Should().Be(dto.FrequencyMHz);
        inserted.MinDbuVPerM.Should().Be(dto.MinDbuVPerM);
        inserted.MaxDbuVPerM.Should().Be(dto.MaxDbuVPerM);
        inserted.IsActive.Should().Be(dto.IsActive);
        inserted.Notes.Should().Be(dto.Notes);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNullAndDoesNotUpdate()
    {
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(null!);

        var result = await CreateSut().UpdateAsync(Guid.NewGuid(), ValidDto());

        result.Should().BeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<ReferenceThreshold>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_MapsDtoOntoExistingAndUpdates()
    {
        var existing = new ReferenceThreshold { Id = Guid.NewGuid(), Notes = "old" };
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(existing);
        _repo.SetupUpdateEcho();
        var dto = ValidDto();

        var result = await CreateSut().UpdateAsync(existing.Id, dto);

        result.Should().BeSameAs(existing);
        existing.Notes.Should().Be(dto.Notes);
        existing.FrequencyMHz.Should().Be(dto.FrequencyMHz);
        existing.MaxDbuVPerM.Should().Be(dto.MaxDbuVPerM);
        _repo.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(null!);

        var result = await CreateSut().DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<ReferenceThreshold>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var existing = new ReferenceThreshold { Id = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        _repo.SetupGet<ReferenceThreshold, ReferenceThreshold>(existing);
        _repo.SetupDeleteEcho();

        var result = await CreateSut().DeleteAsync(existing.Id, userId);

        result.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(existing, userId), Times.Once);
    }
}
