using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthCheckApi.Auth.Abstractions;
using HealthCheckApi.Entity;
using HealthCheckApi.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace HealthCheckApi.Auth;

internal sealed class TokenManager : ITokenManager
{
    private readonly IConfiguration _configuration;

    public TokenManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(UserEntity user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? string.Empty));

        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            new(ClaimTypes.Role, "USER")
        };

        var expirationTime = jwtSettings.GetValue<int>("ExpirationTimeInMinutes");

        var token = new JwtSecurityToken(
            issuer: jwtSettings.GetValue<string>("Issuer"),
            audience: jwtSettings.GetValue<string>("Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationTime),
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    public string GenerateRefreshToken(UserEntity user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? string.Empty));

        var claims = new List<Claim>()
        {
            new (JwtRegisteredClaimNames.Sub, user.Email),
            new (JwtRegisteredClaimNames.Jti, user.Id.ToString())
        };

        var expirationTime = jwtSettings.GetValue<int>("RefreshExpirationTimeInMinutes");

        var token = new JwtSecurityToken(
            issuer: jwtSettings.GetValue<string>("Issuer"),
            audience: jwtSettings.GetValue<string>("Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationTime),
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(bool isValid, string? userEmail)> ValidateRefreshToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        var tokenParameters = TokenHelper.ValidateToken(_configuration);
        var validTokenResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, tokenParameters);

        if (!validTokenResult.IsValid)
            return (false, null);

        var userEmail = validTokenResult.Claims.FirstOrDefault(c => c.Key == ClaimTypes.NameIdentifier).Value as string;

        return (true, userEmail);
    }
}
