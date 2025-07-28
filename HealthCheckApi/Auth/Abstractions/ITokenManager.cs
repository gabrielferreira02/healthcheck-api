using HealthCheckApi.Entity;

namespace HealthCheckApi.Auth.Abstractions;

public interface ITokenManager
{
    string GenerateToken(UserEntity user);
    string GenerateRefreshToken(UserEntity user);
    Task<(bool isValid, string? userEmail)> ValidarRefreshToken(string token);
}
