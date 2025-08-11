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
using Microsoft.OpenApi.Models;

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

        // Add Authentication scheme for scalar
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe o token JWT no formato: Bearer {seu_token}"
                };

                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
