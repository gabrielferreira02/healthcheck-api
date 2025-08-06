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

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateUserRequest>> _validatorMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _validatorMock = new Mock<IValidator<CreateUserRequest>>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _cacheMock = new Mock<ICacheService>();

        _userService = new UserService(
            _repositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task ShouldCreateANewUserWithSuccess()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(r => r.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _repositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity u, CancellationToken _) => u);

        var result = await _userService.CreateUser(request, CancellationToken.None);

        Assert.True(result.IsT0);
        var user = result.AsT0;
        Assert.Equal(request.Email, user.Email);
        Assert.Equal(request.Username, user.Username);
    }

    [Fact]
    public async Task ShouldFailOnCreateANewUserBecauseEmailAlreadyRegistered()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password()
        );
        var userWithEMail = new UserEntity(
            faker.Name.FullName(),
            request.Email,
            faker.Internet.Password()
        );

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(r => r.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userWithEMail);

        var result = await _userService.CreateUser(request, CancellationToken.None);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.BusinessRule, error.Type);
    }

    [Fact]
    public async Task ShouldFaildOnCreateANewUserBecauseEmailIsEmpty()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Name.FullName(),
            "",
            faker.Internet.Password()
        );

        var validationErrors = new List<ValidationFailure>
        {
            new("Email", "Email is not valid")
        };

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationErrors));

        var result = await _userService.CreateUser(request, CancellationToken.None);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFaildOnCreateANewUserBecauseUsernameIsEmpty()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            "",
            faker.Internet.Email(),
            faker.Internet.Password()
        );

        var validationErrors = new List<ValidationFailure>
        {
            new("Username", "Username is not valid")
        };

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationErrors));

        var result = await _userService.CreateUser(request, CancellationToken.None);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldFaildOnCreateANewUserBecausePasswordIsEmpty()
    {
        var faker = new Faker();
        var request = new CreateUserRequest(
            faker.Name.FullName(),
            faker.Internet.Email(),
            ""
        );

        var validationErrors = new List<ValidationFailure>
        {
            new("Password", "Password is not valid")
        };

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationErrors));

        var result = await _userService.CreateUser(request, CancellationToken.None);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.Validation, error.Type);
    }

    [Fact]
    public async Task ShouldDeleteAUserWithSuccess()
    {
        var userId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(r => r.DeleteUser(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock.Setup(c => c.RemoveAsync($"user_{userId}"))
            .Returns(Task.CompletedTask);

        await _userService.DeleteUser(userId, CancellationToken.None);

        _repositoryMock.Verify(r => r.DeleteUser(userId, CancellationToken.None), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync($"user_{userId}"), Times.Once);
    }

    [Fact]
    public async Task ShouldGetUserByIdFromCache()
    {
        var faker = new Faker();
        var userId = Guid.NewGuid();
        var response = new UserResponse(userId, faker.Name.FullName(), faker.Internet.Email());

        _cacheMock.Setup(c => c.GetAsync<UserResponse>($"user_{userId}"))
            .ReturnsAsync(response);

        var result = await _userService.GetUserById(userId, CancellationToken.None);

        Assert.True(result.IsT0);
        var user = result.AsT0;
        Assert.Equal(response.Email, user.Email);
        Assert.Equal(response.Username, user.Username);
        _cacheMock.Verify(c => c.GetAsync<UserResponse>($"user_{userId}"), Times.Once);
    }

    [Fact]
    public async Task ShouldGetUserByIdFromDatabase()
    {
        var faker = new Faker();
        var userId = Guid.NewGuid();
        var response = new UserEntity(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password());

        _cacheMock.Setup(c => c.GetAsync<UserResponse>($"user_{userId}"))
            .ReturnsAsync((UserResponse?)null);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _userService.GetUserById(userId, CancellationToken.None);

        Assert.True(result.IsT0);
        var user = result.AsT0;
        Assert.Equal(response.Email, user.Email);
        Assert.Equal(response.Username, user.Username);
        _cacheMock.Verify(c => c.GetAsync<UserResponse>($"user_{userId}"), Times.Once);
        _cacheMock.Verify(c => c.SetAsync<UserResponse>($"user_{userId}", It.IsAny<UserResponse>(), TimeSpan.FromMinutes(3)));
        _repositoryMock.Verify(r => r.GetUserByIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ShouldFailOnGetUserByIdBecauseIdDoesNotExists()
    {
        var faker = new Faker();
        var userId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync<UserResponse>($"user_{userId}"))
            .ReturnsAsync((UserResponse?)null);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _userService.GetUserById(userId, CancellationToken.None);

        Assert.True(result.IsT1);
        var error = result.AsT1;
        Assert.Equal(TypeErrors.NotFound, error.Type);
        _cacheMock.Verify(c => c.GetAsync<UserResponse>($"user_{userId}"), Times.Once);
        _repositoryMock.Verify(r => r.GetUserByIdAsync(userId, CancellationToken.None), Times.Once);
    }
}
