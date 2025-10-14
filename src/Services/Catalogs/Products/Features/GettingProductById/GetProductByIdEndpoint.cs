using GenAIEshop.Catalogs.Products.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.GettingProductById;

public static class GetProductByIdEndpoint
{
    public static RouteHandlerBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{productId:guid}", HandleAsync)
            .WithName(nameof(GetProductById))
            .WithDisplayName("Get Product By ID")
            .WithSummary("Retrieves a product by its unique identifier.")
            .WithDescription("Returns the product details.")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Ok<ProductDto>, NotFound>> HandleAsync(
        [AsParameters] GetProductByIdRequestParameters parameters
    )
    {
        var (productId, sender, ct) = parameters;

        var query = new GetProductById(productId);
        var result = await sender.Send(query, ct);

        return result.Product != null ? TypedResults.Ok(result.Product) : TypedResults.NotFound();
    }
}

public sealed record GetProductByIdRequestParameters(
    [FromRoute] Guid ProductId,
    ISender Sender,
    CancellationToken CancellationToken
);
