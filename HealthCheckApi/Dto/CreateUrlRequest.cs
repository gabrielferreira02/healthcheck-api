namespace HealthCheckApi.Dto;

public record class CreateUrlRequest(
    Guid UserId,
    string Url,
    int Interval
);
