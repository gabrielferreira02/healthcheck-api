
using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Repository.Abstractions;
using MassTransit;

namespace HealthCheckApi.Services;

public class UrlHealthChecker : BackgroundService
{
    private readonly ILogger<UrlHealthChecker> _logger;
    private readonly IServiceScopeFactory _service;
    private readonly IBus _bus;

    public UrlHealthChecker(
        ILogger<UrlHealthChecker> logger,
        IServiceScopeFactory service,
        IBus bus)
    {
        _logger = logger;
        _service = service;
        _bus = bus;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckUrls(stoppingToken);
            await Task.Delay(60000, stoppingToken);
        }
    }

    public async Task CheckUrls(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Verificação iniciada");

        using var scope = _service.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUrlRepository>();

        var urls = await repository.GetUrlsToCheckAsync(stoppingToken);

        foreach (var url in urls)
        {
            _logger.LogInformation("Verificando: {url}", url.Url);
            var result = await VerifyUrl(url.Url);

            if (result != url.LastStatus)
            {
                _logger.LogInformation("Status alterado da url: {url} de {prev} para {curr}", url.Url, url.LastStatus, result);
                url.UpdateStatus(result);
                var payload = new EmailPayload(url.UserId, url.Url, result, DateTime.Now);
                await _bus.Publish(payload, stoppingToken);
            }

            url.UpdateNextCheck();
            await repository.UpdateUrlAsync(url, stoppingToken);
            _logger.LogInformation("Verificação finalizada");
        }
    }

    private static async Task<HealthStatus> VerifyUrl(string url)
    {
        try
        {
            using var http = new HttpClient();
            var response = await http.GetAsync(url);
            return response.IsSuccessStatusCode ? HealthStatus.UP : HealthStatus.DOWN;
        }
        catch
        {
            return HealthStatus.UNREACHABLE;
        }
    }
}
