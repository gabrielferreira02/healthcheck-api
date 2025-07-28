namespace HealthCheckApi.Dto;

public record class LoginResponse(string Token, string RefreshToken);
