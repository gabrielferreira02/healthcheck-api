using HealthCheckApi.Data;
using HealthCheckApi.Entity;
using HealthCheckApi.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckApi.Repository;

internal sealed class UrlRepository : IUrlRepository
{

    private readonly AppDbContext _context;

    public UrlRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UrlEntity> CreateUrlAsync(UrlEntity url, CancellationToken ct = default)
    {
        await _context.Urls.AddAsync(url, ct);
        await _context.SaveChangesAsync(ct);
        return url;
    }

    public async Task DeleteUrl(Guid id, CancellationToken ct = default)
    {
        var url = await _context.Urls.FindAsync(id, ct) ?? throw new InvalidOperationException("Url not found");
        _context.Urls.Remove(url);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<UrlEntity?> GetUrlByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Urls.FindAsync(id, ct);
    }

    public async Task<UrlEntity?> GetUrlByUrlAddressAndUserIdAsync(string url, Guid UserId, CancellationToken ct = default)
    {
        return await _context.Urls.AsNoTracking().FirstOrDefaultAsync(u => u.Url.Equals(url) && u.UserId == UserId);
    }

    public async Task<List<UrlEntity>?> GetUrlsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Urls.Where(u => u.UserId == userId).ToListAsync();
    }

    public async Task<UrlEntity> UpdateUrlAsync(UrlEntity url, CancellationToken ct = default)
    {
        _context.Urls.Update(url);
        await _context.SaveChangesAsync(ct);
        return url;
    }

    public async Task<List<UrlEntity>> GetUrlsToCheckAsync(CancellationToken ct = default)
    {
        var urls = await _context.Urls.Where(x => x.NextCheck <= DateTime.Now).ToListAsync(ct);
        return urls;
    }
}
