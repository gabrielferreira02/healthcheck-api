using Bogus;
using FluentValidation;
using FluentValidation.Results;
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services;
using HealthCheckApi.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCheckApi.Tests.Services;

public class UrlServiceTest
{
    private readonly Mock<IUrlRepository> _urlRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<CreateUrlRequest>> _validatorCreateUrlMock;
    private readonly Mock<IValidator<UpdateUrlRequest>> _validatorUpdateUrlMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<UrlService>> _loggerMock;
    private readonly UrlService _serviceMock;

    public UrlServiceTest()
    {
        _urlRepositoryMock = new Mock<IUrlRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validatorCreateUrlMock = new Mock<IValidator<CreateUrlRequest>>();
        _validatorUpdateUrlMock = new Mock<IValidator<UpdateUrlRequest>>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<UrlService>>();

        _serviceMock = new UrlService(
            _urlRepositoryMock.Object,
            _validatorCreateUrlMock.Object,
            _userRepositoryMock.Object,
            _validatorUpdateUrlMock.Object,
            _loggerMock.Object,
            _cacheMock.Object
        );
    }

    [Fact]
    public async Task ShouldCreateANewUrlWithSucces()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            "https://www.google.com",
            faker.Random.Int(1, int.MaxValue));

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(request.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new UserEntity(
                    faker.Name.FullName(),
                    faker.Internet.Email(),
                    faker.Internet.Password())
            );

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _urlRepositoryMock.Setup(r => r.CreateUrlAsync(It.IsAny<UrlEntity>(), default))
            .ReturnsAsync(new UrlEntity(request.UserId, request.Url, request.Interval));

        _urlRepositoryMock.Setup(r => r.GetUrlByUrlAddressAndUserIdAsync(request.Url, request.UserId, default))
            .ReturnsAsync((UrlEntity?)null);

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT0);
        var url = result.AsT0;
        Assert.Equal(request.Url, url.Url);
        Assert.Equal(request.UserId, url.UserId);
        Assert.Equal(request.Interval, url.Interval);
        Assert.Equal(HealthStatus.UP, url.LastStatus);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUrlBecauseUseridIsNotValid()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.Empty,
            "https://www.google.com",
            faker.Random.Int(1, int.MaxValue));

        var errors = new List<ValidationFailure>()
        {
            new("UserId", "User id field cannot be empty")
        };

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUrlBecauseUrlIsEmpty()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            "",
            faker.Random.Int(1, int.MaxValue));

        var errors = new List<ValidationFailure>()
        {
            new("Url", "Url field cannot be empty")
        };

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUrlBecauseIntervalIsInvalid()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            "https://www.google.com",
            faker.Random.Int(int.MinValue, 0));

        var errors = new List<ValidationFailure>()
        {
            new("Url", "Url field cannot be empty")
        };

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUrlBecauseUrlIsInvalid()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            faker.Person.UserName,
            faker.Random.Int(1, int.MaxValue));

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.BusinessRule, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUrlBecauseUserNotFound()
    {
        var faker = new Faker();
        var request = new CreateUrlRequest(
            Guid.NewGuid(),
            "https://www.google.com",
            faker.Random.Int(1, int.MaxValue));

        _validatorCreateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(request.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _serviceMock.CreateUrl(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.NotFound, error.Type);
    }

    [Fact]
    public async Task ShouldDeleteAUrlWithSuccess()
    {
        var faker = new Faker();
        var url = new UrlEntity(Guid.NewGuid(), faker.Internet.Url(), faker.Random.Int(1, int.MaxValue));

        _urlRepositoryMock.Setup(r => r.DeleteUrl(url.Id, It.IsAny<CancellationToken>()));

        _urlRepositoryMock.Setup(r => r.GetUrlByIdAsync(url.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(url);

        await _serviceMock.DeleteUrl(url.Id, default);

        _urlRepositoryMock.Verify(r => r.DeleteUrl(url.Id, default), Times.Once);
    }

    [Fact]
    public async Task ShouldFailOnDeleteUrlBecauseIdWasNotFound()
    {
        var urlId = Guid.NewGuid();

        _urlRepositoryMock.Setup(r => r.DeleteUrl(urlId, It.IsAny<CancellationToken>()));

        _urlRepositoryMock.Setup(r => r.GetUrlByIdAsync(urlId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((UrlEntity?)null);

        await _serviceMock.DeleteUrl(urlId, default);

        _urlRepositoryMock.Verify(r => r.DeleteUrl(urlId, default), Times.Never);
    }

    [Fact]
    public async Task ShouldGetUrlByIdWithSuccessFromCache()
    {
        var faker = new Faker();
        var url = new UrlResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.PickRandom<HealthStatus>(),
            faker.Random.Int(1, int.MaxValue));

        _cacheMock.Setup(c => c.GetAsync<UrlResponse>($"url_{url.Id}"))
            .ReturnsAsync(url);

        var result = await _serviceMock.GetUrlById(url.Id, default);

        Assert.True(result.IsT0);
        var urlResponse = result.AsT0;
        Assert.Equal(url.Id, urlResponse.Id);
        Assert.Equal(url.UserId, urlResponse.UserId);
        Assert.Equal(url.Url, urlResponse.Url);
        Assert.Equal(url.Interval, urlResponse.Interval);
    }

    [Fact]
    public async Task ShouldGetUrlByIdWithSuccessFromDatabase()
    {
        var faker = new Faker();
        var url = new UrlEntity(
            Guid.NewGuid(),
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );

        _cacheMock.Setup(c => c.GetAsync<UrlResponse>($"url_{url.Id}"))
            .ReturnsAsync((UrlResponse?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlByIdAsync(url.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        var result = await _serviceMock.GetUrlById(url.Id, default);

        Assert.True(result.IsT0);
        var urlResponse = result.AsT0;
        Assert.Equal(url.Id, urlResponse.Id);
        Assert.Equal(url.UserId, urlResponse.UserId);
        Assert.Equal(url.Url, urlResponse.Url);
        Assert.Equal(url.Interval, urlResponse.Interval);
    }

    [Fact]
    public async Task ShouldFailOnGetUrlByIdBecauseUrlWasNotFound()
    {
        var id = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync<UrlResponse>($"url_{id}"))
            .ReturnsAsync((UrlResponse?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlEntity?)null);

        var result = await _serviceMock.GetUrlById(id, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.NotFound, error.Type);
    }

    [Fact]
    public async Task ShouldGetUserUrlsByUserIdWithSuccessFromCache()
    {
        var faker = new Faker();
        var userId = Guid.NewGuid();

        var url1 = new UrlResponse(
            Guid.NewGuid(),
            userId,
            faker.Internet.Url(),
            faker.PickRandom<HealthStatus>(),
            faker.Random.Int(1, int.MaxValue));
        var url2 = new UrlResponse(
            Guid.NewGuid(),
            userId,
            faker.Internet.Url(),
            faker.PickRandom<HealthStatus>(),
            faker.Random.Int(1, int.MaxValue));

        var urlsList = new List<UrlResponse>
        {
            url1, url2
        };

        _cacheMock.Setup(c => c.GetAsync<List<UrlResponse>>($"user_urls_{userId}"))
            .ReturnsAsync(urlsList);

        var result = await _serviceMock.GetUrlsByUserId(userId, default);

        Assert.NotNull(result);
        Assert.Equal(urlsList.Count, result.Count);
    }

    [Fact]
    public async Task ShouldGetUserUrlsByUserIdWithSuccessFromDatabase()
    {
        var faker = new Faker();
        var userId = Guid.NewGuid();
        var url1 = new UrlEntity(
            userId,
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );
        var url2 = new UrlEntity(
            userId,
            faker.Internet.Url(),
            faker.Random.Int(1, int.MaxValue)
        );
        var urlsList = new List<UrlEntity>
        {
            url1,url2
        };

        _cacheMock.Setup(c => c.GetAsync<List<UrlResponse>>($"user_urls_{userId}"))
            .ReturnsAsync((List<UrlResponse>?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(urlsList);

        var result = await _serviceMock.GetUrlsByUserId(userId, default);

        Assert.NotNull(result);
        Assert.Equal(urlsList.Count, result.Count);
    }

    [Fact]
    public async Task ShouldReturnEmptyListFOrUserUrlsWithUserIdNotFound()
    {
        var userId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync<List<UrlResponse>>($"user_urls_{userId}"))
            .ReturnsAsync((List<UrlResponse>?)null);

        _urlRepositoryMock.Setup(r => r.GetUrlsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UrlEntity>());

        var result = await _serviceMock.GetUrlsByUserId(userId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // UPDATE URL

    [Fact]
    public async Task ShouldUpdateAUrlWithSucces()
    {
        var faker = new Faker();

        var url = new UrlEntity(
            Guid.NewGuid(),
            "https://www.microsoft.com",
            faker.Random.Int(1, int.MaxValue));

        var request = new UpdateUrlRequest(
            url.Id,
            "https://www.google.com",
            faker.Random.Int(1, int.MaxValue));

        var updatedUrl = new UrlEntity(
            url.UserId,
            request.NewUrl,
            request.Interval);

        _urlRepositoryMock.Setup(r => r.GetUrlByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _validatorUpdateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());


        _urlRepositoryMock.Setup(r => r.UpdateUrlAsync(updatedUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUrl);

        _cacheMock.Setup(c => c.RemoveAsync($"url_{request.Id}"))
            .Returns(Task.CompletedTask);

        var result = await _serviceMock.UpdateUrlById(request, default);

        Assert.True(result.IsT0);
        var urlResponse = result.AsT0;
        Assert.Equal(request.NewUrl, urlResponse.Url);
        Assert.Equal(request.Interval, urlResponse.Interval);
    }

    [Fact]
    public async Task ShouldFailOnUpdateUrlBecauseUrlIdIsNotValid()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.Empty,
            "https://www.google.com",
            faker.Random.Int(1, int.MaxValue));

        var errors = new List<ValidationFailure>()
        {
            new("Id", "Url id field cannot be empty")
        };

        _validatorUpdateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.UpdateUrlById(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnUpdateAUrlBecauseNewUrlIsEmpty()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.NewGuid(),
            "",
            faker.Random.Int(1, int.MaxValue));

        var errors = new List<ValidationFailure>()
        {
            new("Url", "Url field cannot be empty")
        };

        _validatorUpdateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.UpdateUrlById(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnUpdateUrlBecauseIntervalIsInvalid()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.NewGuid(),
            "https://www.google.com",
            faker.Random.Int(int.MinValue, 0));

        var errors = new List<ValidationFailure>()
        {
            new("Interval", "Interval must be between 1 minute and 24 hours(in minutes)")
        };

        _validatorUpdateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(errors));

        var result = await _serviceMock.UpdateUrlById(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFailOnUpdateANewUrlBecauseNewUrlIsInvalid()
    {
        var faker = new Faker();
        var request = new UpdateUrlRequest(
            Guid.NewGuid(),
            faker.Person.UserName,
            faker.Random.Int(1, int.MaxValue));

        _validatorUpdateUrlMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var result = await _serviceMock.UpdateUrlById(request, default);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.BusinessRule, error.Type);
    }
}