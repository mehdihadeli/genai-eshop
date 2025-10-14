using BuildingBlocks.EF.Extensions;
using BuildingBlocks.Extensions;
using GenAIEshop.Orders.Orders;
using GenAIEshop.Orders.Shared.Clients;
using GenAIEshop.Orders.Shared.Contracts;
using GenAIEshop.Orders.Shared.Data;
using GenAIEshop.Shared.Constants;

namespace GenAIEshop.Orders.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddOrdersServices();

        AddClients(builder);

        AddDatabase(builder);

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOrdersEndpoints();

        return endpoints;
    }

    private static IHttpClientBuilder AddClients(IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(
            (sp, client) =>
            {
                // https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview
                // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
                // Logical name, resolved by service discovery support for an http client
                client.BaseAddress = new Uri(
                    // catalogs-api
                    $"https+http://{AspireApplicationResources.Api.CatalogsApi}"
                );
            }
        );

        return builder.Services.AddHttpClient<ICartsServiceClient, CartsServiceClient>(
            (sp, client) =>
            {
                // https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview
                // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
                // Logical name, resolved by service discovery support for an http client
                client.BaseAddress = new Uri(
                    // catalogs-api
                    $"https+http://{AspireApplicationResources.Api.CartsApi}"
                );
            }
        );
    }

    private static void AddDatabase(WebApplicationBuilder builder)
    {
        builder.AddPostgresDbContext<OrdersDbContext>(
            connectionStringName: AspireApplicationResources.PostgresDatabase.Orders,
            action: app =>
            {
                if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
                {
                    // apply migration and seed data for dev environment
                    app.AddMigration<OrdersDbContext>();
                }
                else
                {
                    // just apply migration for production without seeding
                    app.AddMigration<OrdersDbContext>();
                }
            }
        );
    }
}
