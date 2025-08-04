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
        Guid userId,
        string url,
        int interval
    )
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty");

        if(string.IsNullOrEmpty(url))
            throw new ArgumentException("Url cannot be empty");

        if(interval <= 0)
            throw new ArgumentException("Interval must be greater than 0");

        Id = Guid.NewGuid();
        UserId = userId;
        Url = url;
        Interval = interval;
        LastStatus = HealthStatus.UP;
        NextCheck = DateTime.Now.AddMinutes(Interval);
        CreatedAt = DateTime.Now;
    }

    public void UpdateUrl(string url)
    {
        Url = url;
    }

    public void UpdateInterval(int interval)
    {
        if(interval <= 0)
            throw new InvalidDataException("Interval must be greater than 0");
            
        Interval = interval;
    }

    public void UpdateStatus(HealthStatus status)
    {
        LastStatus = status;
    }

    public void UpdateNextCheck()
    {
        NextCheck = DateTime.Now.AddMinutes(Interval);
    }

}
