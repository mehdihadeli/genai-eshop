using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Carts.Carts.Features.ClearingCart;

public static class ClearCartEndpoint
{
    public static RouteHandlerBuilder MapClearCartEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/", HandleAsync)
            .WithName(nameof(ClearCart))
            .WithDisplayName("Clear Cart")
            .WithSummary("Clear the user's cart completely.")
            .WithDescription("Deletes all items and removes the cart from cache. Safe to call even if cart is empty.")
            .Produces(StatusCodes.Status204NoContent);
    }

    static async Task<NoContent> HandleAsync([AsParameters] ClearCartRequestParameters parameters)
    {
        var (userId, sender, ct) = parameters;
        var command = ClearCart.Of(userId);
        await sender.Send(command, ct);

        return TypedResults.NoContent();
    }
}

public sealed record ClearCartRequestParameters(
    [FromQuery] Guid UserId,
    ISender Sender,
    CancellationToken CancellationToken
);
