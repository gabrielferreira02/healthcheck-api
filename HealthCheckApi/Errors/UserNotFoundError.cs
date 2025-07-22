using HealthCheckApi.Errors;

namespace HealthCheckApi.Enums;

public record UserNotFoundError() : AppError("User not found", TypeErrors.NotFound);
