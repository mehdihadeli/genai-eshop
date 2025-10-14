using GenAIEshop.Orders.Orders.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Orders.Orders.Features.GettingOrder;

public static class GetOrderEndpoint
{
    public static RouteHandlerBuilder MapGetOrderEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{orderId:guid}", HandleAsync)
            .WithName(nameof(GetOrder))
            .WithDisplayName("Get Order")
            .WithSummary("Retrieves an order by ID.")
            .WithDescription("Returns order details including items, status, and concurrency Version (xmin).")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Ok<OrderDto>, NotFound>> HandleAsync([AsParameters] GetOrderRequestParameters parameters)
    {
        var (orderId, userId, sender, ct) = parameters;

        var query = GetOrder.Of(orderId, userId);
        var result = await sender.Send(query, ct);

        return result.Order != null ? TypedResults.Ok(result.Order) : TypedResults.NotFound();
    }
}

public sealed record GetOrderRequestParameters(
    [FromRoute] Guid OrderId,
    [FromQuery] Guid UserId,
    ISender Sender,
    CancellationToken CancellationToken
);
