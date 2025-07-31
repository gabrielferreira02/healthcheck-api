using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class InvalidTokenError() : AppError("Token cannot be empty", TypeErrors.BusinessRule);
