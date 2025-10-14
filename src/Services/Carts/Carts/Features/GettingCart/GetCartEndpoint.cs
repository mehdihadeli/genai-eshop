using GenAIEshop.Carts.Carts.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Carts.Carts.Features.GettingCart;

public static class GetCartEndpoint
{
    public static RouteHandlerBuilder MapGetCartEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/", HandleAsync)
            .WithName(nameof(GetCart))
            .WithDisplayName("Get User Cart")
            .WithSummary("Retrieves the current shopping cart for a user.")
            .WithDescription("Returns the user's active cart with all items. Returns empty if no cart exists.")
            .Produces<CartDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Ok<CartDto>, NotFound>> HandleAsync([AsParameters] GetCartRequestParameters parameters)
    {
        var (userId, sender, ct) = parameters;

        var result = await sender.Send(GetCart.Of(userId), ct);

        return result.Cart is not null ? TypedResults.Ok(result.Cart) : TypedResults.NotFound();
    }
}

public sealed record GetCartRequestParameters(
    [FromQuery] Guid UserId,
    ISender Sender,
    CancellationToken CancellationToken
);
