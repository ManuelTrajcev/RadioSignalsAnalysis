using Domain.Domain_Models;
using Domain.DTO;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Repository.Interface;
using Services.Interface;
using Xunit;

namespace Services.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new(MockBehavior.Strict);

    private UserService CreateSut() => new(_repo.Object);

    private static RegisterDto ValidRegisterDto() => new()
    {
        Name = "Ana",
        Surname = "Petrova",
        Username = "ana",
        Password = "Secret123!",
        RepeatPassword = "Secret123!",
        Email = "ana@example.com",
        Role = Role.USER
    };

    [Fact]
    public async Task RegisterAsync_WhenUsernameTaken_ReturnsFailureAndDoesNotCreate()
    {
        var dto = ValidRegisterDto();
        _repo.Setup(r => r.FindByNameAsync(dto.Username)).ReturnsAsync(new User { UserName = dto.Username });

        var result = await CreateSut().RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Username already exists.");
        _repo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenPasswordsDoNotMatch_ReturnsFailure()
    {
        var dto = ValidRegisterDto();
        dto.RepeatPassword = "Different!";
        _repo.Setup(r => r.FindByNameAsync(dto.Username)).ReturnsAsync((User?)null);

        var result = await CreateSut().RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Passwords do not match.");
        _repo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ReturnsAggregatedErrorsAndSkipsRole()
    {
        var dto = ValidRegisterDto();
        _repo.Setup(r => r.FindByNameAsync(dto.Username)).ReturnsAsync((User?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password too weak" },
                new IdentityError { Description = "Email invalid" }));

        var result = await CreateSut().RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Password too weak, Email invalid");
        _repo.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenRoleAssignmentFails_ReturnsFailure()
    {
        var dto = ValidRegisterDto();
        _repo.Setup(r => r.FindByNameAsync(dto.Username)).ReturnsAsync((User?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<User>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
        _repo.Setup(r => r.AddToRoleAsync(It.IsAny<User>(), "USER"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role missing" }));

        var result = await CreateSut().RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Role missing");
    }

    [Fact]
    public async Task RegisterAsync_HappyPath_CreatesUserWithDtoFieldsAndAssignsRole()
    {
        var dto = ValidRegisterDto();
        dto.Role = Role.ADMIN;
        User? captured = null;
        _repo.Setup(r => r.FindByNameAsync(dto.Username)).ReturnsAsync((User?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<User>(), dto.Password))
            .Callback<User, string>((u, _) => captured = u)
            .ReturnsAsync(IdentityResult.Success);
        _repo.Setup(r => r.AddToRoleAsync(It.IsAny<User>(), "ADMIN")).ReturnsAsync(IdentityResult.Success);

        var result = await CreateSut().RegisterAsync(dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Registration successful.");
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("ana");
        captured.Email.Should().Be("ana@example.com");
        captured.Name.Should().Be("Ana");
        captured.Surname.Should().Be("Petrova");
        _repo.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), "ADMIN"), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.FindByNameAsync("ghost")).ReturnsAsync((User?)null);

        var result = await CreateSut().AuthenticateAsync("ghost", "whatever");

        result.Should().BeNull();
        _repo.Verify(r => r.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ReturnsNull()
    {
        var user = new User { UserName = "ana" };
        _repo.Setup(r => r.FindByNameAsync("ana")).ReturnsAsync(user);
        _repo.Setup(r => r.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var result = await CreateSut().AuthenticateAsync("ana", "wrong");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenCredentialsValid_ReturnsUser()
    {
        var user = new User { UserName = "ana" };
        _repo.Setup(r => r.FindByNameAsync("ana")).ReturnsAsync(user);
        _repo.Setup(r => r.CheckPasswordAsync(user, "right")).ReturnsAsync(true);

        var result = await CreateSut().AuthenticateAsync("ana", "right");

        result.Should().BeSameAs(user);
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsRolesFromRepository()
    {
        var user = new User { UserName = "ana" };
        var roles = new List<string> { "ADMIN", "USER" };
        _repo.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(roles);

        var result = await CreateSut().GetRolesAsync(user);

        result.Should().BeEquivalentTo(roles);
    }
}
