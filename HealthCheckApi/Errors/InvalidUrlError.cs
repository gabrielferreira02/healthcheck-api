using HealthCheckApi.Enums;

namespace HealthCheckApi.Errors;

public record class InvalidUrlError() : AppError("Invalid Url, try passing a http or https url", TypeErrors.BusinessRule);
