using HealthCheckApi.Entity;

namespace HealthCheckApi.Repository.Abstractions;

public interface IUrlRepository
{
    Task<List<UrlEntity>?> GetUrlsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UrlEntity?> GetUrlByIdAsync(Guid id, CancellationToken ct = default);
    Task<UrlEntity?> GetUrlByUrlAddressAndUserIdAsync(string url, Guid UserId, CancellationToken ct = default);
    Task<UrlEntity> CreateUrlAsync(UrlEntity url, CancellationToken ct = default);
    Task<UrlEntity> UpdateUrlAsync(UrlEntity url, CancellationToken ct = default);
    Task DeleteUrl(Guid id, CancellationToken ct = default);
    Task<List<UrlEntity>> GetUrlsToCheckAsync(CancellationToken ct = default);

}
