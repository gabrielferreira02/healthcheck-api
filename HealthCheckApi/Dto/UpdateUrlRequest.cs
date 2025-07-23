namespace HealthCheckApi.Dto;

public record class UpdateUrlRequest(
    Guid UserId,
    string NewUrl,
    int Interval
);
