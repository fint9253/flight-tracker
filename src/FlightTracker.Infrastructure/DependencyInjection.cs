using FlightTracker.Core.Interfaces;
using FlightTracker.Infrastructure.Data;
using FlightTracker.Infrastructure.Repositories;
using FlightTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace FlightTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<FlightTrackerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<ITrackedFlightRepository, TrackedFlightRepository>();
        services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
        services.AddScoped<IPriceAlertRepository, PriceAlertRepository>();
        services.AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>();

        // External Services with Polly resilience policies
        services.AddHttpClient<IFlightPriceService, AmadeusApiClient>()
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AmadeusApiClient>>();

                // Retry policy: 3 attempts with exponential backoff
                var retryPolicy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            logger.LogWarning(
                                "Amadeus API retry attempt {RetryAttempt} after {Delay}s. Status: {StatusCode}, Reason: {Reason}",
                                retryAttempt,
                                timespan.TotalSeconds,
                                outcome.Result?.StatusCode,
                                outcome.Exception?.Message ?? "Transient HTTP error");
                        });

                return retryPolicy;
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AmadeusApiClient>>();

                // Circuit breaker: Break after 5 consecutive failures, break for 30 seconds
                var circuitBreakerPolicy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: 5,
                        durationOfBreak: TimeSpan.FromSeconds(30),
                        onBreak: (outcome, duration) =>
                        {
                            logger.LogError(
                                "Amadeus API circuit breaker opened for {Duration}s. Status: {StatusCode}, Reason: {Reason}",
                                duration.TotalSeconds,
                                outcome.Result?.StatusCode,
                                outcome.Exception?.Message ?? "Transient HTTP error");
                        },
                        onReset: () =>
                        {
                            logger.LogInformation("Amadeus API circuit breaker reset (closed)");
                        },
                        onHalfOpen: () =>
                        {
                            logger.LogInformation("Amadeus API circuit breaker is half-open, testing connection");
                        });

                return circuitBreakerPolicy;
            })
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));

        // Caching
        services.AddMemoryCache();

        // Background Services
        services.AddHostedService<PricePollingService>();

        return services;
    }
}
