using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Features.GettingProductById;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.CreatingProduct;

public static class CreateProductEndpoint
{
    public static RouteHandlerBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/", HandleAsync)
            .WithName(nameof(CreateProduct))
            .WithDisplayName("Create Product")
            .WithSummary("Creates a new product in the catalog.")
            .WithDescription("Adds a new product with the specified details.")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<CreatedAtRoute<ProductDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] CreateProductRequestParameters parameters
    )
    {
        var (request, sender, ct) = parameters;

        var command = CreateProduct.Of(
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.IsAvailable
        );

        var result = await sender.Send(command, ct);

        return TypedResults.CreatedAtRoute(
            result.Product,
            nameof(GetProductById),
            new { productId = result.Product.Id }
        );
    }
}

public sealed record CreateProductRequestParameters(
    [FromBody] CreateProductRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record CreateProductRequest(
    string? Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable = true
);
