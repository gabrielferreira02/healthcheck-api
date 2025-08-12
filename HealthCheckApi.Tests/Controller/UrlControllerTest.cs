using Bogus;
using HealthCheckApi.Controller;
using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthCheckApi.Tests.Controller;

public class UrlControllerTest
{
    private readonly Mock<IUrlService> _service;
    private readonly UrlController _controller;

    public UrlControllerTest()
    {
        _service = new Mock<IUrlService>();
        _controller = new(_service.Object);
    }

    [Fact]
    public async Task ShouldReturnUrlsByUserIdWithSuccess()
    {
        var faker = new Faker();
        var id = Guid.NewGuid();
        var urls = new List<UrlResponse>()
        {
            new(Guid.NewGuid(), id, faker.Internet.Url(), faker.PickRandom<HealthStatus>(), faker.Random.Int(1, int.MaxValue)),
            new(Guid.NewGuid(), id, faker.Internet.Url(), faker.PickRandom<HealthStatus>(), faker.Random.Int(1, int.MaxValue)),
            new(Guid.NewGuid(), id, faker.Internet.Url(), faker.PickRandom<HealthStatus>(), faker.Random.Int(1, int.MaxValue))
        };

        _service.Setup(s => s.GetUrlsByUserId(id, default))
            .ReturnsAsync(urls);

        var result = await _controller.GetUserUrls(id, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(urls, okObject.Value);
    }

    [Fact]
    public async Task ShouldGetUrlByIdWithSuccess()
    {
        var faker = new Faker();
        var urlId = Guid.NewGuid();
        var response = new UrlResponse(urlId, Guid.NewGuid(), faker.Internet.Url(), faker.PickRandom<HealthStatus>(), faker.Random.Int(1, int.MaxValue));

        _service.Setup(s => s.GetUrlById(urlId, default))
            .ReturnsAsync(response);

        var result = await _controller.GetUrlById(urlId, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(response, okObject.Value);
    }

    [Fact]
    public async Task ShouldFailOnGetUrlsByIdBecauseIdWasNotFound()
    {
        var id = Guid.NewGuid();
        var response = new UrlNotFoundError();

        _service.Setup(s => s.GetUrlById(id, default))
        .ReturnsAsync(response);

        var result = await _controller.GetUrlById(id, default);

        var errorObject = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldDeleteAUrl()
    {
        var id = Guid.NewGuid();

        _service.Setup(s => s.DeleteUrl(id, default))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUrl(id, default);

        var okObject = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, okObject.StatusCode);
    }

    [Fact]
    public async Task ShouldCreateAUrlWithSuccess()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new UrlResponse(
            Guid.NewGuid(),
            request.UserId,
            request.Url,
            faker.PickRandom<HealthStatus>(),
            request.Interval
        );

        _service.Setup(s => s.CreateUrl(request, default))
            .ReturnsAsync(response);

        var result = await _controller.CreateUrl(request, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(response, okObject.Value);
    }

    [Fact]
    public async Task ShouldFailOnCreateAUrlBecauseRequestIsNotValid()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.Empty,
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new ValidationError(new ValidationProblemDetails());

        _service.Setup(s => s.CreateUrl(request, default))
            .ReturnsAsync(response);

        var result = await _controller.CreateUrl(request, default);

        var errorObject = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldFailOnCreateAUrlBecauseUrlAddressAlreadyRegisteredByUser()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.Empty,
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new UrlAlreadyRegisteredByUserError();

        _service.Setup(s => s.CreateUrl(request, default))
            .ReturnsAsync(response);

        var result = await _controller.CreateUrl(request, default);

        var errorObject = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldFailOnCreateAUrlBecauseUserIdWasNotFound()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new UrlNotFoundError();

        _service.Setup(s => s.CreateUrl(request, default))
            .ReturnsAsync(response);

        var result = await _controller.CreateUrl(request, default);

        var errorObject = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdateAUrlWithSuccess()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new UrlResponse(
            request.Id,
            Guid.NewGuid(),
            request.NewUrl,
            faker.PickRandom<HealthStatus>(),
            request.Interval
        );

        _service.Setup(s => s.UpdateUrlById(request, default))
            .ReturnsAsync(response);

        var result = await _controller.UpdateUrl(request, default);

        var okObject = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okObject.StatusCode);
        Assert.Equal(response, okObject.Value);
    }

    [Fact]
    public async Task ShouldFailOnUpdateAUrlBecauseRequestIsNotValid()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.Empty,
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new ValidationError(new ValidationProblemDetails());

        _service.Setup(s => s.UpdateUrlById(request, default))
            .ReturnsAsync(response);

        var result = await _controller.UpdateUrl(request, default);

        var errorObject = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, errorObject.StatusCode);
    }

    [Fact]
    public async Task ShouldFailOnUpdateAUrlBecauseUrlIdWasNotFound()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        var response = new UrlNotFoundError();

        _service.Setup(s => s.UpdateUrlById(request, default))
            .ReturnsAsync(response);

        var result = await _controller.UpdateUrl(request, default);

        var errorObject = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, errorObject.StatusCode);
    }
}
