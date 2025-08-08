using System.Text.Json;
using HealthCheckApi.Services.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthCheckApi.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            return data != null ? JsonSerializer.Deserialize<T>(data!) : default;
        }
        catch (Exception)
        {
            _logger.LogError("Erro ao obter cache");
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expirationTime)
    {
        try
        {
            var valueSerialized = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            };
            await _cache.SetStringAsync(key, valueSerialized, options);
        }
        catch (Exception)
        {
            _logger.LogError("Erro ao definir cache");
        }
    }
}
