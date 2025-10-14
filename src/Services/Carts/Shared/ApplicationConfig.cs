using GenAIEshop.Carts.Carts;
using GenAIEshop.Carts.Shared.Clients;
using GenAIEshop.Carts.Shared.Contracts;
using GenAIEshop.Shared.Constants;

namespace GenAIEshop.Carts.Shared;

public static class ApplicationConfig
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddCartsServices();

        AddClients(builder);

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCartsEndpoints();

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
