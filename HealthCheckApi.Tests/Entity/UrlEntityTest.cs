using Bogus;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;

namespace HealthCheckApi.Tests.Entity;

public class UrlEntityTest
{
    [Fact]
    public void ShouldCreateNewUrlEntityWithSuccess()
    {
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);

        Assert.NotNull(newUrlEntity);
        Assert.Equal(userId, newUrlEntity.UserId);
        Assert.Equal(interval, newUrlEntity.Interval);
        Assert.Equal(url, newUrlEntity.Url);
        Assert.Equal(HealthStatus.UP, newUrlEntity.LastStatus);
    }

    [Fact]
    public void ShouldFailOnCreateUrlEntityBecauseIntervalIsNegativeOrZero()
    {
        var faker = new Faker();
        int interval = faker.Random.Int(int.MinValue, 0);
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();

        Assert.Throws<ArgumentException>(() =>
        {
            new UrlEntity(userId, url, interval);
        });
    }

    [Fact]
    public void ShouldFailOnCreateUrlEntityBecauseUrlIsEmpty()
    {
        var faker = new Faker();
        int interval = faker.Random.Int(1, int.MaxValue);
        Guid userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
        {
            new UrlEntity(userId, "", interval);
        });
    }

    [Fact]
    public void ShouldFailOnCreateUrlEntityBecauseUserIdIsEmpty()
    {
        var faker = new Faker();
        int interval = faker.Random.Int(1, int.MaxValue);
        var url = faker.Internet.Url();

        Assert.Throws<ArgumentException>(() =>
        {
            new UrlEntity(Guid.Empty, url, interval);
        });
    }

    [Fact]
    public void ShouldUpdateStatusWithSuccess()
    {
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);
        var urlEntity = new UrlEntity(userId, url, interval);
        var newStatus = HealthStatus.UNREACHABLE;

        urlEntity.UpdateStatus(newStatus);

        Assert.Equal(newStatus, urlEntity.LastStatus);
    }

    [Fact]
    public void ShouldUpdateUrlWithSuccess()
    {
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);
        var urlEntity = new UrlEntity(userId, url, interval);
        var newUrl = faker.Internet.Url();

        urlEntity.UpdateUrl(newUrl);

        Assert.Equal(newUrl, urlEntity.Url);
    }

    [Fact]
    public void ShouldUpdateIntervalWithSuccess()
    {
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);
        var urlEntity = new UrlEntity(userId, url, interval);
        var newInterval = faker.Random.Int(1, int.MaxValue);

        urlEntity.UpdateInterval(newInterval);

        Assert.Equal(newInterval, urlEntity.Interval);
    }

    [Fact]
    public void ShouldFailOnUpdateStatusBecauseValueIsNegative()
    {
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int();
        var urlEntity = new UrlEntity(userId, url, interval);
        var newInterval = faker.Random.Int(int.MinValue, 0);

        Assert.Throws<InvalidDataException>(() =>
        {
            urlEntity.UpdateInterval(newInterval);
        });

    }
}
