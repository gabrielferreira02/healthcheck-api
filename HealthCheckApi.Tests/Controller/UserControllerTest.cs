using System.ComponentModel.DataAnnotations;
using Bogus;
using HealthCheckApi.Controller;
using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthCheckApi.Tests.Controller;

public class UserControllerTest
{
    private readonly Mock<IUserService> _service;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _service = new Mock<IUserService>();
        _controller = new(_service.Object);
    }

    [Fact]
    public async Task ShouldCreateAUserWithSuccesAndReturnOkStatus()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Person.UserName,
            faker.Person.Email,
            faker.Internet.Password()
        );

        var response = new UserResponse(
            Guid.NewGuid(),
            request.Username,
            request.Email
        );

        _service.Setup(s => s.CreateUser(request, default))
        .ReturnsAsync(response);

        var result = await _controller.CreateUser(request, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(response, okObject.Value);
    }

    [Fact]
    public async Task ShouldFailOnCreateAUserBecauseEmailAlreadyExistsAndReturnsBadRequestStatus()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Person.UserName,
            faker.Person.Email,
            faker.Internet.Password()
        );

        var response = new EmailAlreadyExistsError();

        _service.Setup(s => s.CreateUser(request, default))
        .ReturnsAsync(response);

        var result = await _controller.CreateUser(request, default);

        var errorObject = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldFailOnCreateAUserBecauseRequestHasInvalidDataAndReturnsValidationProblemStatus()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            "",
            faker.Person.Email,
            faker.Internet.Password()
        );

        var errors = new ValidationProblemDetails();
        var response = new ValidationError(errors);

        _service.Setup(s => s.CreateUser(request, default))
        .ReturnsAsync(response);

        var result = await _controller.CreateUser(request, default);

        var errorObject = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldDeleteAUserAndReturnNoContentStatus()
    {
        var id = Guid.NewGuid();

        _service.Setup(s => s.DeleteUser(id, default))
        .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(id, default);

        var okObject = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, okObject.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnAUserByIdWithSuccess()
    {
        var faker = new Faker();
        var response = new UserResponse(
            Guid.NewGuid(),
            faker.Person.UserName,
            faker.Person.Email
        );

        _service.Setup(s => s.GetUserById(response.Id, default))
            .ReturnsAsync(response);

        var result = await _controller.GetUserById(response.Id, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(response, okObject.Value);
    }

    [Fact]
    public async Task ShouldFailOnReturnAUserByIdBecauseIdWasNotFoundAndReturnNotFoundStatus()
    {
        var id = Guid.NewGuid();

        _service.Setup(s => s.GetUserById(id, default))
            .ReturnsAsync(new UserNotFoundError());

        var result = await _controller.GetUserById(id, default);

        var notFoundObject = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundObject.StatusCode);
    }
}
