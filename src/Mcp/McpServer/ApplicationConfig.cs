using GenAIEshop.Shared.Constants;
using McpServer.Shared.Clients;
using McpServer.Shared.Contracts;

namespace GenAIEshop.Carts.Shared;

public static class ApplicationConfig
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        AddClients(builder);

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }

    private static IHttpClientBuilder AddClients(IHostApplicationBuilder builder)
    {
        return builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(
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
    }
}
