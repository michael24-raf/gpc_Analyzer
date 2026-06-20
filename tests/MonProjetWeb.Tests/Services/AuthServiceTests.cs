using FluentAssertions;
using Moq;
using MonProjetWeb.Application.Common.DTOs.Auth;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Infrastructure.Services;
using MonProjetWeb.Tests.Helpers;

namespace MonProjetWeb.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtService> _jwtMock;

    public AuthServiceTests()
    {
        _jwtMock = new Mock<IJwtService>();
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");
    }

    [Fact]
    public async Task Register_WithNewEmail_ShouldSucceed()
    {
        // Arrange
        using var db      = TestDbContextFactory.Create();
        var authService   = new AuthService(db, _jwtMock.Object);
        var dto           = new RegisterDto
        {
            FirstName       = "Jean",
            LastName        = "Dupont",
            Email           = "jean@test.com",
            Password        = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().Be("fake-jwt-token");
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("jean@test.com");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldFail()
    {
        // Arrange
        using var db    = TestDbContextFactory.Create();
        var authService = new AuthService(db, _jwtMock.Object);

        // Créer un utilisateur existant
        TestDataSeeder.CreateUser(db, "existing@test.com");

        var dto = new RegisterDto
        {
            FirstName       = "Marie",
            LastName        = "Martin",
            Email           = "existing@test.com",
            Password        = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await authService.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("déjà utilisé");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        using var db    = TestDbContextFactory.Create();
        var authService = new AuthService(db, _jwtMock.Object);

        TestDataSeeder.CreateUser(db, "login@test.com");

        var dto = new LoginDto
        {
            Email    = "login@test.com",
            Password = "Password123!"
        };

        // Act
        var result = await authService.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().Be("fake-jwt-token");
        result.User!.Email.Should().Be("login@test.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldFail()
    {
        // Arrange
        using var db    = TestDbContextFactory.Create();
        var authService = new AuthService(db, _jwtMock.Object);

        TestDataSeeder.CreateUser(db, "user@test.com");

        var dto = new LoginDto
        {
            Email    = "user@test.com",
            Password = "WrongPassword!"
        };

        // Act
        var result = await authService.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("incorrect");
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldFail()
    {
        // Arrange
        using var db    = TestDbContextFactory.Create();
        var authService = new AuthService(db, _jwtMock.Object);

        var dto = new LoginDto
        {
            Email    = "nobody@test.com",
            Password = "Password123!"
        };

        // Act
        var result = await authService.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
    }
}