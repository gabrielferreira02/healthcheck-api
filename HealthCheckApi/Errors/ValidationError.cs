using HealthCheckApi.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckApi.Errors;

public record class ValidationError(ValidationProblemDetails Validation) : AppError(Validation, TypeErrors.Validation);
