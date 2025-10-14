using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;

namespace BuildingBlocks.Extensions;

public static class ServiceDiscoveryExtensions
{
    public static string? GetEndpointAddress(this IServiceProvider sp, string endpointDiscoveryAddress)
    {
        // it adds `ServiceEndpointResolver` in AddServiceDiscoveryCore when we register `http.AddServiceDiscovery()` for all httpclients using `ConfigureHttpClientDefaults`
        // var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
        // https://github.com/dotnet/aspnetcore/issues/53715
        var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
        var endpoints = resolver
            .GetEndpointsAsync(endpointDiscoveryAddress, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        var hostEndpoint = endpoints.Endpoints.FirstOrDefault()?.EndPoint.ToString();
        
        return hostEndpoint;
    }
}