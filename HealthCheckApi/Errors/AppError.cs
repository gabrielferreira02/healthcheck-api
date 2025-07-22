using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class AppError(object Error, TypeErrors Type);
