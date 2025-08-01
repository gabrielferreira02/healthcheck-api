using FluentValidation;
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using StackExchange.Redis;

namespace HealthCheckApi.Services;

public sealed class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IValidator<CreateUrlRequest> _createUrlValidator;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateUrlRequest> _updateUrlValidator;
    private readonly ILogger<UrlService> _logger;
    private readonly ICacheService _cache;

    public UrlService(
        IUrlRepository urlRepository,
        IValidator<CreateUrlRequest> createUrlValidator,
        IUserRepository userRepository,
        IValidator<UpdateUrlRequest> updateUrlValidator,
        ILogger<UrlService> logger,
        ICacheService cache)
    {
        _urlRepository = urlRepository;
        _createUrlValidator = createUrlValidator;
        _userRepository = userRepository;
        _updateUrlValidator = updateUrlValidator;
        _logger = logger;
        _cache = cache;
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

            _logger.LogWarning("Falha ao criar url. Erros de validação");
            return new ValidationError(problemDetails);
        }

        if (!IsValidUrl(request.Url))
        {
            _logger.LogWarning("Formato de url invalida para: {url}", request.Url);
            return new InvalidUrlError();
        }

        var user = await _userRepository.GetUserByIdAsync(request.UserId, ct);

        if (user is null)
        {
            _logger.LogWarning("Usuário com id {id} não encontrado", request.UserId);
            return new UserNotFoundError();
        }

        var url = await _urlRepository.GetUrlByUrlAddressAndUserIdAsync(request.Url, request.UserId, ct);

        if (url is not null)
        {
            _logger.LogWarning("Url {url} já cadastrada pelo usuario {id}", request.Url, request.UserId);
            return new UrlAlreadyRegisteredByUserError();
        }

        var newUrl = new UrlEntity(request.UserId, request.Url, request.Interval);
        await _urlRepository.CreateUrlAsync(newUrl, ct);

        _logger.LogInformation("Url {url} cadastrado pelo usuário {user}", request.Url, request.UserId);
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
        var url = await _urlRepository.GetUrlByIdAsync(id, ct);
        
        if (url != null)
        {
            await _urlRepository.DeleteUrl(id, ct);
            await _cache.RemoveAsync($"url_{id}");
            await _cache.RemoveAsync($"user_urls_{url.UserId}");
            _logger.LogInformation("Url com id {id} deletada", id);
        }
    }

    public async Task<OneOf<UrlResponse, AppError>> GetUrlById(Guid id, CancellationToken ct = default)
    {
        var urlCache = await _cache.GetAsync<UrlResponse>($"url_{id}");

        if (urlCache != null)
        {
            _logger.LogInformation("Dados da url {id} retornados do cache", id);
            return urlCache;    
        }

        var url = await _urlRepository.GetUrlByIdAsync(id, ct);

        if (url is null)
        {
            _logger.LogWarning("Url com id {id} não encontrada", id);
            return new UrlNotFoundError();
        }

        var response = new UrlResponse(url.Id, url.UserId, url.Url, url.LastStatus, url.Interval);
        await _cache.SetAsync<UrlResponse>($"url_{id}", response, TimeSpan.FromMinutes(3));

        _logger.LogInformation("Url com id {id} encontrada", id);
        return response;
    }

    public async Task<List<UrlResponse>?> GetUrlsByUserId(Guid userId, CancellationToken ct = default)
    {
        var urlsCache = await _cache.GetAsync<List<UrlResponse>>($"user_urls_{userId}");

        if (urlsCache != null) {
            _logger.LogInformation("Urls do usuário {id} retornadas do cache", userId);
            return urlsCache;
        }

        var urls = await _urlRepository.GetUrlsByUserIdAsync(userId, ct) ?? [];
        var urlsFormated = new List<UrlResponse>();
        
        foreach (var url in urls)
        {
            urlsFormated.Add(new UrlResponse(url.Id, url.UserId, url.Url, url.LastStatus, url.Interval));
        }

        if (urls.Count > 0)
            await _cache.SetAsync<List<UrlResponse>>($"user_urls_{userId}", urlsFormated, TimeSpan.FromMinutes(3));

        _logger.LogInformation("Retornado urls do usuário {id}", userId);
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

            _logger.LogWarning("Falha ao criar url. Erros de validação");
            return new ValidationError(problemDetails);
        }

        if (!IsValidUrl(request.NewUrl))
        {
            _logger.LogWarning("Formato de url invalida para: {url}", request.NewUrl);
            return new InvalidUrlError();
        }


        var url = await _urlRepository.GetUrlByIdAsync(request.Id, ct);

        if (url is null)
        {
            _logger.LogWarning("Url com id {id} não encontrada", request.Id);
            return new UrlNotFoundError();
        }

        url.UpdateUrl(request.NewUrl);
        url.UpdateInterval(request.Interval);
        await _urlRepository.UpdateUrlAsync(url, ct);
        await _cache.RemoveAsync($"user_urls_{url.UserId}");

        _logger.LogInformation("Url com id {id} atualizada", request.Id);
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
