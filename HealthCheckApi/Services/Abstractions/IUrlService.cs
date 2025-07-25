using HealthCheckApi.Dto;
using HealthCheckApi.Errors;
using OneOf;

namespace HealthCheckApi.Services.Abstractions;

public interface IUrlService
{
    Task<OneOf<UrlResponse, AppError>> CreateUrl(CreateUrlRequest request, CancellationToken ct = default);
    Task<List<UrlResponse>?> GetUrlsByUserId(Guid userId, CancellationToken ct = default);
    Task<OneOf<UrlResponse, AppError>> GetUrlById(Guid id, CancellationToken ct = default);
    Task<OneOf<UrlResponse, AppError>> UpdateUrlById(UpdateUrlRequest request, CancellationToken ct = default);
    Task DeleteUrl(Guid id, CancellationToken ct = default);
}
