using GenAIEshop.Catalogs.Products.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.UpdatingProduct;

public static class UpdateProductEndpoint
{
    public static RouteHandlerBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/{productId:guid}", HandleAsync)
            .WithName(nameof(UpdateProduct))
            .WithDisplayName("Update Product")
            .WithSummary("Updates an existing product in the catalog.")
            .WithDescription("Updates product details.")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .Produces<ProblemHttpResult>(StatusCodes.Status404NotFound)
            .Produces<ProblemHttpResult>(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<ProductDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] UpdateProductRequestParameters parameters
    )
    {
        var (productId, request, sender, ct) = parameters;

        var command = UpdateProduct.Of(
            productId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.IsAvailable
        );

        var result = await sender.Send(command, ct);
        return TypedResults.Ok(result.Product);
    }
}

public sealed record UpdateProductRequestParameters(
    [FromRoute] Guid ProductId,
    [FromBody] UpdateProductRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable
);
