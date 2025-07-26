using HealthCheckApi.Enums;

namespace HealthCheckApi.Dto;

public record class EmailPayload(
    Guid UserId,
    string Url,
    HealthStatus Status,
    DateTime VerifiedAt
);
