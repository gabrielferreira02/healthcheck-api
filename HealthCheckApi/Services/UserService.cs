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

    public UserService(
        IUserRepository repository,
        IValidator<CreateUserRequest> createUserValidator)
    {
        _repository = repository;
        _createUserValidator = createUserValidator;
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

            return new ValidationError(problemDetails);
        }

        var userWithEmail = await _repository.GetUserByEmail(request.Email, ct);

        if (userWithEmail is not null)
            return new EmailAlreadyExistsError();

        var password = PasswordHasher.HashPassword(request.Password);
        var user = new UserEntity(request.Username, request.Email, password);
        var newUser = await _repository.CreateUserAsync(user, ct);

        return new UserResponse(newUser.Id, newUser.Username, newUser.Email);
    }

    public async Task DeleteUser(Guid id, CancellationToken ct)
    {
        await _repository.DeleteUser(id, ct);
    }

    public async Task<OneOf<UserResponse, AppError>> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await _repository.GetUserByIdAsync(id, ct);

        if (user is null)
            return new UserNotFoundError();

        return new UserResponse(user.Id, user.Username, user.Email);
    }
}
