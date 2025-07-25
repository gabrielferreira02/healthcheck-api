using FluentValidation;
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace HealthCheckApi.Services;

public sealed class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IValidator<CreateUrlRequest> _createUrlValidator;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateUrlRequest> _updateUrlValidator;

    public UrlService(
        IUrlRepository urlRepository,
        IValidator<CreateUrlRequest> createUrlValidator,
        IUserRepository userRepository,
        IValidator<UpdateUrlRequest> updateUrlValidator)
    {
        _urlRepository = urlRepository;
        _createUrlValidator = createUrlValidator;
        _userRepository = userRepository;
        _updateUrlValidator = updateUrlValidator;
    }

    public async Task<OneOf<UrlResponse, AppError>> CreateUrl(CreateUrlRequest request, CancellationToken ct = default)
    {
        var validation = await _createUrlValidator.ValidateAsync(request, ct);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errors);

            return new ValidationError(problemDetails);
        }

        if (!IsValidUrl(request.Url))
            return new InvalidUrlError();

        var user = await _userRepository.GetUserByIdAsync(request.UserId, ct);

        if (user is null)
            return new UserNotFoundError();

        var url = await _urlRepository.GetUrlByUrlAddressAndUserIdAsync(request.Url, request.UserId, ct);

        if (url is not null)
            return new UrlAlreadyRegisteredByUserError();

        var newUrl = new UrlEntity(request.UserId, request.Url, request.Interval);
        await _urlRepository.CreateUrlAsync(newUrl, ct);

        return new UrlResponse(
            newUrl.Id,
            newUrl.UserId,
            newUrl.Url,
            newUrl.LastStatus,
            newUrl.Interval
        );
    }

    public async Task DeleteUrl(Guid id, CancellationToken ct = default)
    {
        await _urlRepository.DeleteUrl(id, ct);
    }

    public async Task<OneOf<UrlResponse, AppError>> GetUrlById(Guid id, CancellationToken ct = default)
    {
        var url = await _urlRepository.GetUrlByIdAsync(id, ct);

        if (url is null)
            return new UrlNotFoundError();

        return new UrlResponse(url.Id, url.UserId, url.Url, url.LastStatus, url.Interval);
    }

    public async Task<List<UrlResponse>?> GetUrlsByUserId(Guid userId, CancellationToken ct = default)
    {
        var urls = await _urlRepository.GetUrlsByUserIdAsync(userId, ct) ?? new List<UrlEntity>();
        var urlsFormated = new List<UrlResponse>();
        
        foreach (var url in urls)
        {
            urlsFormated.Add(new UrlResponse(url.Id, url.UserId, url.Url, url.LastStatus, url.Interval));
        }

        return urlsFormated;
    }

    public async Task<OneOf<UrlResponse, AppError>> UpdateUrlById(UpdateUrlRequest request, CancellationToken ct = default)
    {
        var validation = await _updateUrlValidator.ValidateAsync(request, ct);

        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errors);

            return new ValidationError(problemDetails);
        }

        if (!IsValidUrl(request.NewUrl))
            return new InvalidUrlError();


        var url = await _urlRepository.GetUrlByIdAsync(request.Id, ct);

        if (url is null)
            return new UrlNotFoundError();

        url.UpdateUrl(request.NewUrl);
        url.UpdateInterval(request.Interval);
        await _urlRepository.UpdateUrlAsync(url, ct);

        return new UrlResponse(
            url.Id,
            url.UserId,
            url.Url,
            url.LastStatus,
            url.Interval
        );
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
