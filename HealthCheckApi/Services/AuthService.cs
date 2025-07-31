using HealthCheckApi.Auth.Abstractions;
using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Helpers;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services.Abstractions;
using OneOf;

namespace HealthCheckApi.Services;

public class AuthService : IAuthService
{
    private readonly ITokenManager _tokenManager;
    private readonly IUserRepository _repository;
    private readonly IConfiguration _configuration;

    public AuthService(
        ITokenManager tokenManager,
        IUserRepository repository,
        IConfiguration configuration)
    {
        _tokenManager = tokenManager;
        _repository = repository;
        _configuration = configuration;
    }
    public async Task<OneOf<LoginResponse, AppError>> Login(LoginRequest request, CancellationToken ct = default)
    {
        var passwordEncoded = PasswordHasher.HashPassword(request.Password);
        var user = await _repository.GetUserByEmail(request.Email);

        if (user is null)
            return new UserNotFoundError();

        var isValidPassword = PasswordHasher.VerifyPassword(passwordEncoded, request.Password);

        if (!isValidPassword)
            return new UnauthorizedError();

        var token = _tokenManager.GenerateToken(user);
        var refreshToken = _tokenManager.GenerateRefreshToken(user);

        return new LoginResponse(token, refreshToken);
    }

    public async Task<OneOf<LoginResponse, AppError>> RefreshToken(RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.Token))
            return new InvalidTokenError();

        var isValidToken = await _tokenManager.ValidateRefreshToken(request.Token);

        if (!isValidToken.isValid)
            return new UnauthorizedError();

        var user = await _repository.GetUserByEmail(isValidToken.userEmail!);

        if (user is null)
            return new UserNotFoundError();

        var token = _tokenManager.GenerateToken(user);
        var refreshToken = _tokenManager.GenerateRefreshToken(user);

        return new LoginResponse(token, refreshToken);
    }
}
