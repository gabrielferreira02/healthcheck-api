using HealthCheckApi.Dto;
using HealthCheckApi.Entity;

namespace HealthCheckApi.Repository.Abstractions;

public interface IUserRepository
{
    Task<UserEntity> CreateUserAsync(UserEntity user, CancellationToken ct = default);
    Task DeleteUser(Guid id, CancellationToken ct);
    Task<UserEntity?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserEntity?> GetUserByEmail(string email, CancellationToken ct = default);
}
