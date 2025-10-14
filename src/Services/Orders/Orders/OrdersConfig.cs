using GenAIEshop.Orders.Orders.Features.CheckoutCart;
using GenAIEshop.Orders.Orders.Features.GettingOrder;
using GenAIEshop.Orders.Orders.Features.UpdatingOrderStatus;
using Humanizer;
using StackExchange.Redis;

namespace GenAIEshop.Orders.Orders;

public static class OrdersConfig
{
    public static IHostApplicationBuilder AddOrdersServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var orders = endpoints.NewVersionedApi(nameof(Order).Pluralize().Kebaberize());
        var ordersV1 = orders.MapGroup("/api/v{version:apiVersion}/orders").HasApiVersion(1.0);

        ordersV1.MapCheckoutEndpoint();
        ordersV1.MapGetOrderEndpoint();
        ordersV1.MapUpdateOrderStatusEndpoint();

        return endpoints;
    }
}
