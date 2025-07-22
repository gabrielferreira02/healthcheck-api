using HealthCheckApi.Enums;

namespace HealthCheckApi.Entity;

public class UrlEntity
{
    public Guid Id { get; init; }
    public UserEntity? User { get; private set; }
    public Guid UserId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int Interval { get; private set; }
    public HealthStatus LastStatus { get; private set; }
    public DateTime NextCheck { get; private set; }
    public DateTime CreatedAt { get; init; }

    public UrlEntity() { }

    public UrlEntity(
        UserEntity user,
        string url,
        int interval
    )
    {
        Id = Guid.NewGuid();
        User = user;
        UserId = user.Id;
        Url = url;
        Interval = interval;
        LastStatus = HealthStatus.UP;
        NextCheck = DateTime.Now.AddMinutes(Interval);
        CreatedAt = DateTime.Now;
    }

}
