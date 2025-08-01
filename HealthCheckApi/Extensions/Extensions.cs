using FluentValidation;
using HealthCheckApi.Auth;
using HealthCheckApi.Auth.Abstractions;
using HealthCheckApi.Consumers;
using HealthCheckApi.Dto;
using HealthCheckApi.Helpers;
using HealthCheckApi.Repository;
using HealthCheckApi.Repository.Abstractions;
using HealthCheckApi.Services;
using HealthCheckApi.Services.Abstractions;
using HealthCheckApi.Validators;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HealthCheckApi.Extensions;

public static class Extensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IUrlRepository, UrlRepository>();
        services.AddScoped<IUrlService, UrlService>();
        services.AddScoped<IValidator<CreateUrlRequest>, CreateUrlRequestValidator>();
        services.AddScoped<IValidator<UpdateUrlRequest>, UpdateUrlRequestValidator>();
        services.AddHostedService<UrlHealthChecker>();
        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<IAuthService, AuthService>();

        // Rabbit Mq Configuration
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

        // Redis config
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "HealthCheckApi";
        });
        services.AddScoped<ICacheService, CacheService>();

        // Authentication and authorization config
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = TokenHelper.ValidateToken(configuration));

        services.AddAuthorization();

        return services;
    }
}
