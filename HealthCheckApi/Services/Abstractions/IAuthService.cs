using HealthCheckApi.Dto;
using HealthCheckApi.Errors;
using OneOf;

namespace HealthCheckApi.Services.Abstractions;

public interface IAuthService
{
    Task<OneOf<LoginResponse, AppError>> Login(LoginRequest request, CancellationToken ct = default);
    Task<OneOf<LoginResponse, AppError>> RefreshToken(RefreshTokenRequest request, CancellationToken ct = default);
}
