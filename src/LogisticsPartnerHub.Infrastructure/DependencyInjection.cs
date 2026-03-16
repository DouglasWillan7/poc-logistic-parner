using LogisticsPartnerHub.Application.Interfaces;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using LogisticsPartnerHub.Infrastructure.BackgroundJobs;
using LogisticsPartnerHub.Infrastructure.Data;
using LogisticsPartnerHub.Infrastructure.Data.Repositories;
using LogisticsPartnerHub.Infrastructure.Http;
using LogisticsPartnerHub.Infrastructure.Http.Authenticators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace LogisticsPartnerHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<LogisticsPartnerDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IFieldMappingRepository, FieldMappingRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IServiceOrderLogRepository, ServiceOrderLogRepository>();
        services.AddScoped<IPartnerEndpointRepository, PartnerEndpointRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Authenticators
        services.AddSingleton<ApiKeyAuthenticator>();
        services.AddSingleton<BasicAuthAuthenticator>();
        services.AddSingleton<OAuth2Authenticator>();
        services.AddSingleton<IPartnerAuthenticator, PartnerAuthenticatorFactory>();

        // HttpClient com Polly - retry com backoff exponencial + circuit breaker
        services.AddHttpClient("PartnerClient")
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddHttpClient("OAuth2Token");

        services.AddScoped<IPartnerHttpClient, PartnerHttpClient>();

        // Monitor notification
        services.AddHttpClient("MonitorClient");
        services.AddScoped<IPartnerNotifier, MonitorNotifier>();

        // Retry queue
        services.AddSingleton<IRetryQueue, InMemoryRetryQueue>();

        // Background jobs
        services.AddHostedService<ServiceOrderProcessorJob>();
        services.AddHostedService<RetryQueueProcessor>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(5, retryAttempt - 1)), // 1s, 5s, 25s
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    // Log será feito pelo Polly internamente
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
