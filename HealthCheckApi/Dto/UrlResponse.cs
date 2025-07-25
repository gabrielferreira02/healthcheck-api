using HealthCheckApi.Enums;

namespace HealthCheckApi.Dto;

public record class UrlResponse(
    Guid Id,
    Guid UserId,
    string Url,
    HealthStatus LastStatus,
    int Interval
);
