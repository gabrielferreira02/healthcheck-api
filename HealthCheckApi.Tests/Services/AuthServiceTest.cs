using System;
using Bogus;
using HealthCheckApi.Auth.Abstractions;
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Helpers;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCheckApi.Tests.Services;

public class AuthServiceTest
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<ITokenManager> _tokenManagerMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _serviceMock;

    public AuthServiceTest()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _tokenManagerMock = new Mock<ITokenManager>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _serviceMock = new(
            _tokenManagerMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ShouldLoginAUserWithSuccess()
    {
        var faker = new Faker();

        var request = new LoginRequest(
            faker.Internet.Email(),
            faker.Internet.Password()
        );

        var user = new UserEntity(
            faker.Name.FullName(),
            request.Email,
            PasswordHasher.HashPassword(request.Password));

        _repositoryMock.Setup(r => r.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenManagerMock.Setup(m => m.GenerateToken(user))
            .Returns("token123");

        _tokenManagerMock.Setup(m => m.GenerateRefreshToken(user))
            .Returns("refresh123");

        var result = await _serviceMock.Login(request, default);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal("token123", response.Token);
        Assert.Equal("refresh123", response.RefreshToken);
    }

    [Fact]
    public async Task ShouldFailOnLoginAUserBecauseUserNotFound()
    {
        var faker = new Faker();

        var request = new LoginRequest(
            "",
            ""
        );

        _repositoryMock.Setup(r => r.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _serviceMock.Login(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.NotFound, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnLoginAUserBecausePasswordIsWrong()
    {
        var faker = new Faker();

        var request = new LoginRequest(
            faker.Person.Email,
            faker.Internet.Password()
        );

        var user = new UserEntity(
            faker.Name.FullName(),
            request.Email,
            PasswordHasher.HashPassword("senha"));

        _repositoryMock.Setup(r => r.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _serviceMock.Login(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Unauthorized, error.Type);
    }

    [Fact]
    public async Task ShouldGenerateANewTokenWithRefreshTokenSuccessfully()
    {
        var faker = new Faker();
        string email = faker.Internet.Email();
        string refreshToken = "refresh123";
        var request = new RefreshTokenRequest(refreshToken);

        var user = new UserEntity(
            faker.Name.FullName(),
            email,
            PasswordHasher.HashPassword(faker.Internet.Password()));

        _tokenManagerMock.Setup(m => m.ValidateRefreshToken(refreshToken))
        .ReturnsAsync((true, email));

        _repositoryMock.Setup(r => r.GetUserByEmail(email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

        _tokenManagerMock.Setup(m => m.GenerateToken(user))
            .Returns("token123");

        _tokenManagerMock.Setup(m => m.GenerateRefreshToken(user))
            .Returns("refreshnovo123");

        var result = await _serviceMock.RefreshToken(request, default);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.Equal("token123", response.Token);
        Assert.Equal("refreshnovo123", response.RefreshToken);
    }

    [Fact]
    public async Task ShouldFailOnRefreshTokenBecauseUserNotFound()
    {
        var faker = new Faker();
        string email = faker.Internet.Email();
        string refreshToken = "refresh123";
        var request = new RefreshTokenRequest(refreshToken);

        _tokenManagerMock.Setup(m => m.ValidateRefreshToken(refreshToken))
            .ReturnsAsync((true, email));

        _repositoryMock.Setup(r => r.GetUserByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _serviceMock.RefreshToken(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.NotFound, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnRefreshTokenBecauseTokenIsInvalid()
    {
        string refreshToken = "refresh123";
        var request = new RefreshTokenRequest(refreshToken);

        _tokenManagerMock.Setup(m => m.ValidateRefreshToken(refreshToken))
            .ReturnsAsync((false, null));

        var result = await _serviceMock.RefreshToken(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Unauthorized, error.Type);
    }

}
