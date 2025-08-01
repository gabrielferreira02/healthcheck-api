using System;

namespace HealthCheckApi.Services.Abstractions;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expirationTime);
    Task RemoveAsync(string key);
}
