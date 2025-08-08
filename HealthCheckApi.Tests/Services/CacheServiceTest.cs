using System.Text;
using System.Text.Json;
using Bogus;
using HealthCheckApi.Dto;
using HealthCheckApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCheckApi.Tests.Services;

public class CacheServiceTest
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private CacheService _servicemock;

    public CacheServiceTest()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheService>>();

        _servicemock = new(
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ShouldGetAKeyWithSuccess()
    {
        var faker = new Faker();

        var user = new UserResponse(
            Guid.NewGuid(),
            faker.Name.FullName(),
            faker.Person.Email
        );
        string key = $"user_{user.Id}";

        _cacheMock.Setup(c => c.GetAsync(key, default))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user)));

        var result = await _servicemock.GetAsync<UserResponse>(key);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task ShouldFailOnGetAKey()
    {
        string key = $"user_{Guid.NewGuid()}";

        _cacheMock.Setup(c => c.GetAsync(key, default))
            .ReturnsAsync((byte[]?)null);

        var result = await _servicemock.GetAsync<UserResponse>(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task ShouldVerifyDeletedMethodWasCalled()
    {
        string key = $"user_{Guid.NewGuid()}";

        _cacheMock.Setup(c => c.RemoveAsync(key, default))
            .Returns(Task.CompletedTask);

        await _servicemock.RemoveAsync(key);

        _cacheMock.Verify(c => c.RemoveAsync(key, default), Times.Once);
    }

    [Fact]
    public async Task ShouldInsertANewKeyWithSuccess()
    {
        var faker = new Faker();
        string key = $"user_{Guid.NewGuid()}";
        var time = TimeSpan.FromMinutes(3);
        var user = new UserResponse(
            Guid.NewGuid(),
            faker.Name.FullName(),
            faker.Internet.Email()
        );

        var serializedValue = JsonSerializer.Serialize(user);

        _cacheMock.Setup(c => c.SetAsync(key, Encoding.UTF8.GetBytes(serializedValue), It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == time), default))
            .Returns(Task.CompletedTask);

        await _servicemock.SetAsync<UserResponse>(key, user, time);

        _cacheMock.Verify(c => c.SetAsync(key, Encoding.UTF8.GetBytes(serializedValue) , It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == time), default), Times.Once);
    }
}
