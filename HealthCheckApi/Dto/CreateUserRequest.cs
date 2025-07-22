namespace HealthCheckApi.Dto;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password);
