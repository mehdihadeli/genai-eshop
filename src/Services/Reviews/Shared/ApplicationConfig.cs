using BuildingBlocks.EF.Extensions;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.ProductReviews;
using GenAIEshop.Reviews.Shared.Clients;
using GenAIEshop.Reviews.Shared.Contracts;
using GenAIEshop.Reviews.Shared.Data;
using GenAIEshop.Shared.Constants;

namespace GenAIEshop.Reviews.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddProductReviewsServices();

        AddDatabase(builder);

        AddClients(builder);

        return builder;
    }

    private static void AddDatabase(WebApplicationBuilder builder)
    {
        builder.AddPostgresDbContext<ReviewsDbContext>(
            connectionStringName: AspireApplicationResources.PostgresDatabase.Reviews,
            action: app =>
            {
                if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
                {
                    // apply migration and seed data for dev environment
                    app.AddMigration<ReviewsDbContext, ReviewsDataSeeder>();
                }
                else
                {
                    // just apply migration for production without seeding
                    app.AddMigration<ReviewsDbContext>();
                }
            }
        );
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapProductReviewsEndpoints();

        return endpoints;
    }

    private static IHttpClientBuilder AddClients(IHostApplicationBuilder builder)
    {
        return builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(
            (sp, client) =>
            {
                // https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview
                // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
                // https://github.com/dotnet/aspnetcore/issues/53715
                // Logical name, resolved by service discovery support for an http client
                // it add `ServiceEndpointResolver` in AddServiceDiscoveryCore when we register `http.AddServiceDiscovery()` for all httpclients using `ConfigureHttpClientDefaults`
                // var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                client.BaseAddress = new Uri(
                    // catalogs-api
                    $"https+http://{AspireApplicationResources.Api.CatalogsApi}"
                );
            }
        );
    }
}
