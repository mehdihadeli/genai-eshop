using BuildingBlocks.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;

namespace Aspire.ServiceDefaults;

// Things that are applied to every microservice but building-blocks not apply to every service.

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddOpenTelemetryDefaults();

        builder.AddHealthChecksDefaults();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddHttpContextAccessor();

        // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#scheme-selection-when-resolving-https-endpoints
        builder.Services.Configure<ServiceDiscoveryOptions>(options => options.AllowAllSchemes = true);

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler(options =>
            {
                var timeSpan = TimeSpan.FromMinutes(2);
                options.AttemptTimeout.Timeout = timeSpan;
                options.CircuitBreaker.SamplingDuration = timeSpan * 2;
                options.TotalRequestTimeout.Timeout = timeSpan * 3;
                options.Retry.MaxRetryAttempts = 1;
            });

            // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
            // Turn on service discovery by default on all http clients
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthCheckEndpoint();

        return app;
    }
}
