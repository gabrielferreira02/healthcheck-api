using System;
using Bogus;
using HealthCheckApi.Data;
using HealthCheckApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckApi.Tests.Repository;

public class UrlRepositoryTest
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ShouldCreateANewUrl()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.Id == newUrlEntity.Id);

        Assert.NotNull(urlCreated);
    }

    [Fact]
    public async Task ShouldUpdateAUrl()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);
        int newInterval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.Id == newUrlEntity.Id);

        Assert.NotNull(urlCreated);
        urlCreated.UpdateInterval(newInterval);
        await context.SaveChangesAsync();

        var updatedUrl = await context.Urls.FirstOrDefaultAsync(u => u.Id == urlCreated.Id);

        Assert.NotNull(updatedUrl);
        Assert.Equal(urlCreated.Interval, updatedUrl.Interval);
        Assert.Equal(urlCreated.Url, updatedUrl.Url);
    }

    [Fact]
    public async Task ShouldDeleteAUrl()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.Id == newUrlEntity.Id);

        Assert.NotNull(urlCreated);
        context.Urls.Remove(urlCreated);
        await context.SaveChangesAsync();

        var urls = await context.Urls.ToListAsync();
        Assert.Empty(urls);
    }

    [Fact]
    public async Task ShouldGetUrlbyId()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.Id == newUrlEntity.Id);

        Assert.NotNull(urlCreated);
    }

    [Fact]
    public async Task ShouldGetUrlByUserId()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.UserId == userId);

        Assert.NotNull(urlCreated);
    }

    [Fact]
    public async Task ShouldGetUrlByUrlAddressAndUserId()
    {
        var context = CreateContext();
        var faker = new Faker();
        Guid userId = Guid.NewGuid();
        var url = faker.Internet.Url();
        int interval = faker.Random.Int(1, int.MaxValue);

        var newUrlEntity = new UrlEntity(userId, url, interval);
        await context.Urls.AddAsync(newUrlEntity);
        await context.SaveChangesAsync();

        var urlCreated = await context.Urls.FirstOrDefaultAsync(u => u.UserId == userId && u.Url.Equals(url));

        Assert.NotNull(urlCreated);
    }

    [Fact]
    public async Task ShouldGetUrlsToCheck()
    {
        var context = CreateContext();
        var faker = new Faker();

        var urlEntity1 = new UrlEntity(
            Guid.NewGuid(),
            faker.Internet.Url(),
            1
        );
        var urlEntity2 = new UrlEntity(
            Guid.NewGuid(),
            faker.Internet.Url(),
            1
        );
        var urlEntity3 = new UrlEntity(
            Guid.NewGuid(),
            faker.Internet.Url(),
            3
        );
        await context.Urls.AddAsync(urlEntity1);
        await context.Urls.AddAsync(urlEntity2);
        await context.Urls.AddAsync(urlEntity3);

        await context.SaveChangesAsync();

        var now = DateTime.Now.AddMinutes(2);
        var urls = await context.Urls.Where(x => x.NextCheck <= now).ToListAsync();

        Assert.NotNull(urls);
        Assert.Equal(2, urls.Count);
        Assert.DoesNotContain(urls, x => x.Id == urlEntity3.Id);
    }
}
