using GenAIEshop.Carts.Carts.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Carts.Carts.Features.UpdatingCart;

public static class UpdateCartEndpoint
{
    public static RouteHandlerBuilder MapUpdateCartEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/", HandleAsync)
            .WithName(nameof(UpdateCart))
            .WithDisplayName("Update Cart")
            .WithSummary("Applies multiple changes to the cart in a single atomic operation.")
            .WithDescription(
                "Supports adding new items, updating quantities, and removing items. "
                    + "All changes are applied atomically. Returns the updated cart state."
            )
            .Produces<CartDto>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .Produces<ProblemHttpResult>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<CartDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] UpdateCartRequestParameters parameters
    )
    {
        var (userId, request, sender, ct) = parameters;

        var command = UpdateCart.Of(userId, request.Changes);

        var result = await sender.Send(command, ct);

        return TypedResults.Ok(result.Cart);
    }
}

public sealed record UpdateCartRequestParameters(
    [FromQuery] Guid UserId,
    [FromBody] UpdateCartRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record UpdateCartRequest(List<CartItemChange>? Changes);

public sealed record CartItemChange(Guid ProductId, string? ProductName, decimal UnitPrice, int Quantity);
