using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class UrlAlreadyRegisteredByUserError() : AppError("Url already registered by user", TypeErrors.BusinessRule);
