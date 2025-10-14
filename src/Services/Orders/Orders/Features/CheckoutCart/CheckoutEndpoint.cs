using GenAIEshop.Orders.Orders.Dtos;
using GenAIEshop.Orders.Orders.Features.GettingOrder;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Orders.Orders.Features.CheckoutCart;

public static class CheckoutEndpoint
{
    public static RouteHandlerBuilder MapCheckoutEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/checkout", HandleAsync)
            .WithName(nameof(CheckoutCarts))
            .WithDisplayName("Checkout")
            .WithSummary("Converts the user's cart into a new order.")
            .WithDescription("Validates all products in cart exist and are available via Catalogs service.")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .Produces<ProblemHttpResult>(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();
    }

    static async Task<Results<CreatedAtRoute<OrderDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] CheckoutRequestParameters parameters
    )
    {
        var (userId, request, sender, ct) = parameters;

        var command = CheckoutCarts.Of(userId, request.ShippingAddress);

        var result = await sender.Send(command, ct);
        return TypedResults.CreatedAtRoute(
            result.Order,
            nameof(GetOrder),
            new { orderId = result.Order.Id, userId = result.Order.UserId }
        );
    }
}

public sealed record CheckoutRequestParameters(
    [FromQuery] Guid UserId,
    [FromBody] CheckoutRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record CheckoutRequest(string? ShippingAddress);
