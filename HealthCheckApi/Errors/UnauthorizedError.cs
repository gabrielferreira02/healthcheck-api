using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class UnauthorizedError() : AppError("", TypeErrors.Unauthorized);
