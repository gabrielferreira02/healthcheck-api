using FluentValidation;
using HealthCheckApi.Consumers;
using HealthCheckApi.Dto;
using HealthCheckApi.Repository;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services;
using HealthCheckApi.Services.Abstractions;
using HealthCheckApi.Validators;
using MassTransit;

namespace HealthCheckApi.Extensions;

public static class Extensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IUrlRepository, UrlRepository>();
        services.AddScoped<IUrlService, UrlService>();
        services.AddScoped<IValidator<CreateUrlRequest>, CreateUrlRequestValidator>();
        services.AddScoped<IValidator<UpdateUrlRequest>, UpdateUrlRequestValidator>();
        services.AddHostedService<UrlHealthChecker>();
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<SimulateSendEmailConsumer>();

            busConfigurator.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri("amqp://localhost:5672"), host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
