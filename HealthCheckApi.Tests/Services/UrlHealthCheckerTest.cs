using HealthCheckApi.Dto;
using HealthCheckApi.Entity;
using HealthCheckApi.Enums;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCheckApi.Tests.Services;

public class UrlHealthCheckerTest
{
    private readonly Mock<ILogger<UrlHealthChecker>> _loggerMock;
    private readonly Mock<IBus> _busMock;
    private readonly Mock<IUrlRepository> _urlRepositoryMock;
    private readonly Mock<IServiceScopeFactory> _serviceFactoryMock;
    private readonly UrlHealthChecker _serviceMock;

    public UrlHealthCheckerTest()
    {
        _serviceFactoryMock = new Mock<IServiceScopeFactory>();
        _urlRepositoryMock = new Mock<IUrlRepository>();
        _loggerMock = new Mock<ILogger<UrlHealthChecker>>();
        _busMock = new Mock<IBus>();

        _serviceMock = new(
            _loggerMock.Object,
            _serviceFactoryMock.Object,
            _busMock.Object
        );
    }

    [Fact]
    public async Task ShouldVerifyUrlsWithSuccessAndHasAUrlToChangeStatus()
    {
        var url = new UrlEntity(
            Guid.NewGuid(),
            "https://www.microsoft.com",
            1
        );
        url.UpdateStatus(HealthStatus.DOWN);

        var urls = new List<UrlEntity>() { url };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IUrlRepository)))
            .Returns(_urlRepositoryMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _serviceFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(serviceScopeMock.Object);

        _urlRepositoryMock.Setup(r => r.GetUrlsToCheckAsync(default))
            .ReturnsAsync(urls);

        _urlRepositoryMock.Setup(r => r.UpdateUrlAsync(It.IsAny<UrlEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _busMock.Setup(b => b.Publish(It.IsAny<EmailPayload>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _serviceMock.CheckUrls(CancellationToken.None);

        _busMock.Verify(b => b.Publish(It.IsAny<EmailPayload>(), It.IsAny<CancellationToken>()), Times.Once);
        _urlRepositoryMock.Verify(r => r.UpdateUrlAsync(It.IsAny<UrlEntity>(), It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task ShouldVerifyUrlsWithSuccessAndNoUrlsToUpdateStatus()
    {
        var url = new UrlEntity(
            Guid.NewGuid(),
            "https://www.microsoft.com",
            1
        );

        var urls = new List<UrlEntity>() { url };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IUrlRepository)))
            .Returns(_urlRepositoryMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _serviceFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(serviceScopeMock.Object);

        _urlRepositoryMock.Setup(r => r.GetUrlsToCheckAsync(default))
            .ReturnsAsync(urls);

        _urlRepositoryMock.Setup(r => r.UpdateUrlAsync(It.IsAny<UrlEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(url);

        _busMock.Setup(b => b.Publish(It.IsAny<EmailPayload>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _serviceMock.CheckUrls(CancellationToken.None);

        _busMock.Verify(b => b.Publish(It.IsAny<EmailPayload>(), It.IsAny<CancellationToken>()), Times.Never);
        _urlRepositoryMock.Verify(r => r.UpdateUrlAsync(It.IsAny<UrlEntity>(), It.IsAny<CancellationToken>()), Times.Once);

    }
}
