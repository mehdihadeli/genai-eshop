using GenAIEshop.Carts.Carts.Features.ClearingCart;
using GenAIEshop.Carts.Carts.Features.GettingCart;
using GenAIEshop.Carts.Carts.Features.UpdatingCart;
using Humanizer;

namespace GenAIEshop.Carts.Carts;

public static class CartsConfig
{
    public static IHostApplicationBuilder AddCartsServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapCartsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var carts = endpoints.NewVersionedApi(nameof(Carts).Pluralize().Kebaberize());
        var cartsV1 = carts.MapGroup("/api/v{version:apiVersion}/carts").HasApiVersion(1.0);

        cartsV1.MapClearCartEndpoint();
        cartsV1.MapUpdateCartEndpoint();
        cartsV1.MapGetCartEndpoint();

        return endpoints;
    }
}
