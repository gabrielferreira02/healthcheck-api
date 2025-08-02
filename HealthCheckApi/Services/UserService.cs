using FluentValidation;
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Helpers;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace HealthCheckApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly ILogger<UserService> _logger;
    private readonly ICacheService _cache;

    public UserService(
        IUserRepository repository,
        IValidator<CreateUserRequest> createUserValidator,
        ILogger<UserService> logger,
        ICacheService cache)
    {
        _repository = repository;
        _createUserValidator = createUserValidator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<OneOf<UserResponse, AppError>> CreateUser(CreateUserRequest request, CancellationToken ct)
    {
        var validation = await _createUserValidator.ValidateAsync(request, ct);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errors);

            _logger.LogWarning("Falha ao criar usuário. Erros de validação");
            return new ValidationError(problemDetails);
        }

        var userWithEmail = await _repository.GetUserByEmail(request.Email, ct);

        if (userWithEmail is not null)
        { 
            _logger.LogWarning("Falha ao criar usuário. Email já cadastrado: {email}", request.Email);
            return new EmailAlreadyExistsError();
        }

        var password = PasswordHasher.HashPassword(request.Password);
        var user = new UserEntity(request.Username, request.Email, password);
        var newUser = await _repository.CreateUserAsync(user, ct);

        _logger.LogInformation("Novo usuário cadastrado com email: {email}", request.Email);
        return new UserResponse(newUser.Id, newUser.Username, newUser.Email);
    }

    public async Task DeleteUser(Guid id, CancellationToken ct)
    {
        await _repository.DeleteUser(id, ct);
        await _cache.RemoveAsync($"user_{id}");
        _logger.LogInformation("Usuário com id {id} deletado", id);
    }

    public async Task<OneOf<UserResponse, AppError>> GetUserById(Guid id, CancellationToken ct)
    {
        var userCache = await _cache.GetAsync<UserResponse>($"user_{id}");

        if (userCache != null)
        {
            _logger.LogInformation("Dados do usuário {id} retornados do cache", id);
            return userCache;
        }

        var user = await _repository.GetUserByIdAsync(id, ct);

        if (user is null)
        {
            _logger.LogWarning("Usuário com id {id} não encontrado", id);
            return new UserNotFoundError();
        }

        var response = new UserResponse(user.Id, user.Username, user.Email);
        await _cache.SetAsync<UserResponse>($"user_{id}", response, TimeSpan.FromMinutes(3));

        _logger.LogInformation("Usuário com id {id} encontrado", id);
        return response;
    }
}
