using GenAIEshop.Orders.Orders.Dtos;
using GenAIEshop.Orders.Orders.Models;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Orders.Orders.Features.UpdatingOrderStatus;

public static class UpdateOrderStatusEndpoint
{
    public static RouteHandlerBuilder MapUpdateOrderStatusEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPatch("/{orderId:guid}/status", HandleAsync)
            .WithName(nameof(UpdateOrderStatus))
            .WithDisplayName("Update Order Status")
            .WithSummary("Updates the status of an order.")
            .WithDescription("Implements state machine validation.")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .Produces<ProblemHttpResult>(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<OrderDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] UpdateOrderStatusRequestParameters parameters
    )
    {
        var (orderId, userId, request, sender, ct) = parameters;

        var command = UpdateOrderStatus.Of(orderId, userId, request.NewStatus);

        var result = await sender.Send(command, ct);
        return TypedResults.Ok(result.Order);
    }
}

public sealed record UpdateOrderStatusRequestParameters(
    [FromRoute] Guid OrderId,
    [FromQuery] Guid UserId,
    [FromBody] UpdateOrderStatusRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record UpdateOrderStatusRequest(OrderStatus NewStatus);
