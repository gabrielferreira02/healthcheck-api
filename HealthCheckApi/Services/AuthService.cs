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
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenManager tokenManager,
        IUserRepository repository,
        ILogger<AuthService> logger)
    {
        _tokenManager = tokenManager;
        _repository = repository;
        _logger = logger;
    }

    public async Task<OneOf<LoginResponse, AppError>> Login(LoginRequest request, CancellationToken ct = default)
    {
        var passwordEncoded = PasswordHasher.HashPassword(request.Password);
        var user = await _repository.GetUserByEmail(request.Email);

        if (user is null)
        {
            _logger.LogWarning("Usuário com email {email} não encontrado", request.Email);
            return new UserNotFoundError();
        }    

        var isValidPassword = PasswordHasher.VerifyPassword(passwordEncoded, request.Password);

        if (!isValidPassword)
        {
            _logger.LogWarning("Senha inválida para tentativa de login do usuário {email}", request.Email);
            return new UnauthorizedError();
        }

        var token = _tokenManager.GenerateToken(user);
        var refreshToken = _tokenManager.GenerateRefreshToken(user);

        _logger.LogInformation("Usuário com email {email} autenticado com sucesso", request.Email);
        return new LoginResponse(token, refreshToken);
    }

    public async Task<OneOf<LoginResponse, AppError>> RefreshToken(RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            _logger.LogWarning("Token nulo ou vazio ao tentar usar refresh");
            return new InvalidTokenError();
        }

        var isValidToken = await _tokenManager.ValidateRefreshToken(request.Token);

        if (!isValidToken.isValid)
        {
            _logger.LogWarning("Token {token} não é válido", request.Token);
            return new UnauthorizedError();
        }

        var user = await _repository.GetUserByEmail(isValidToken.userEmail!);

        if (user is null)
        {
            _logger.LogWarning("Usuário com email {email}, vindo do token, não encontrado", isValidToken.userEmail);
            return new UserNotFoundError();
        }

        var token = _tokenManager.GenerateToken(user);
        var refreshToken = _tokenManager.GenerateRefreshToken(user);

        _logger.LogInformation("Refresh realizado com sucesso para usuário {id}", user.Id);
        return new LoginResponse(token, refreshToken);
    }
}
