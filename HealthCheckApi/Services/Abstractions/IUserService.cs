using HealthCheckApi.Dto;
using HealthCheckApi.Errors;
using OneOf;

namespace HealthCheckApi.Services.Abstractions;

public interface IUserService
{
    Task<OneOf<UserResponse, AppError>> CreateUser(CreateUserRequest request, CancellationToken ct);
    Task DeleteUser(Guid id, CancellationToken ct);
    Task<OneOf<UserResponse, AppError>> GetUserById(Guid id, CancellationToken ct);
}
