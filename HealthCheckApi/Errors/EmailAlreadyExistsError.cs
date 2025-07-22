using HealthCheckApi.Errors;

namespace HealthCheckApi.Enums;

public record EmailAlreadyExistsError() : AppError("Email already registered", TypeErrors.BusinessRule);
