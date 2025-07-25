using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class UrlNotFoundError() : AppError("Url not found", TypeErrors.NotFound);
