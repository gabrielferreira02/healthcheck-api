using System;
using Bogus;
using HealthCheckApi.Controller;
using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthCheckApi.Tests.Controller;

public class AuthControllerTest
{
    private readonly Mock<IAuthService> _service;
    private readonly AuthController _controller;

    public AuthControllerTest()
    {
        _service = new Mock<IAuthService>();

        _controller = new(
            _service.Object
        );
    }

    [Fact]
    public async Task ShouldLoginAUserWithSuccessAndReturnStatusCode200()
    {
        var faker = new Faker();
        var request = new LoginRequest(faker.Person.Email, faker.Internet.Password());
        var response = new LoginResponse("token123", "refresh123");

        _service.Setup(s => s.Login(request, default))
            .ReturnsAsync(response);

        var result = await _controller.Login(request, default);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedOnLoginWhenCredentialsAreInvalid()
    {
        var faker = new Faker();
        var request = new LoginRequest(faker.Person.Email, faker.Internet.Password());
        var response = new UnauthorizedError();

        _service.Setup(s => s.Login(request, default))
            .ReturnsAsync(response);

        var result = await _controller.Login(request, default);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task ShouldReturNotFoundOnLoginWhenCredentialsDoesntExists()
    {
        var faker = new Faker();
        var request = new LoginRequest(faker.Person.Email, faker.Internet.Password());
        var response = new UserNotFoundError();

        _service.Setup(s => s.Login(request, default))
            .ReturnsAsync(response);

        var result = await _controller.Login(request, default);

        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task ShouldRefreshTokenWithSuccessAndReturnStatusCode200()
    {
        var request = new RefreshTokenRequest("refresh123");
        var response = new LoginResponse("novoToken123", "novoRefreshToken123");

        _service.Setup(s => s.RefreshToken(request, default))
        .ReturnsAsync(response);

        var result = await _controller.RefreshToken(request, default);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedOnRefreshnWhenTokenIsInvalid()
    {
        var faker = new Faker();
        var request = new RefreshTokenRequest("refreshTokenInvalido123");
        var response = new UnauthorizedError();

        _service.Setup(s => s.RefreshToken(request, default))
            .ReturnsAsync(response);

        var result = await _controller.RefreshToken(request, default);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task ShouldReturNotFoundOnRefreshWhenCredentialsDoesntExists()
    {
        var request = new RefreshTokenRequest("refreshTokenUsuarioInexistente123");
        var response = new UserNotFoundError();

        _service.Setup(s => s.RefreshToken(request, default))
            .ReturnsAsync(response);

        var result = await _controller.RefreshToken(request, default);

        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenRefreshTokenIsEmpty()
    {
        var request = new RefreshTokenRequest("");
        var response = new InvalidTokenError();

        _service.Setup(s => s.RefreshToken(request, default))
            .ReturnsAsync(response);

        var result = await _controller.RefreshToken(request, default);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
