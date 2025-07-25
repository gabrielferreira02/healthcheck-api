namespace HealthCheckApi.Dto;

public record class UpdateUrlRequest(
    Guid Id,
    string NewUrl,
    int Interval
);
