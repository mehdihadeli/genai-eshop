using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.DeletingProduct;

public static class DeleteProductEndpoint
{
    public static RouteHandlerBuilder MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{productId:guid}", HandleAsync)
            .WithName(nameof(DeleteProduct))
            .WithDisplayName("Delete Product")
            .WithSummary("Deletes a product from the catalog.")
            .WithDescription("Removes a product permanently.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemHttpResult>(StatusCodes.Status404NotFound)
            .Produces<ProblemHttpResult>(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    static async Task<Results<NoContent, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] DeleteProductRequestParameters parameters
    )
    {
        var (productId, sender, ct) = parameters;

        var command = DeleteProduct.Of(productId);

        await sender.Send(command, ct);
        return TypedResults.NoContent();
    }
}

public sealed record DeleteProductRequestParameters(
    [FromRoute] Guid ProductId,
    ISender Sender,
    CancellationToken CancellationToken
);
