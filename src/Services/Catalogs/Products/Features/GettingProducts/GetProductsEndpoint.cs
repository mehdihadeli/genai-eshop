using GenAIEshop.Catalogs.Products.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.GettingProducts;

public static class GetProductsEndpoint
{
    public static RouteHandlerBuilder MapGetProductsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/", HandleAsync)
            .WithName(nameof(GetProducts))
            .WithDisplayName("Get Products")
            .WithSummary("Retrieves a paginated list of products.")
            .WithDescription("Returns products with pagination metadata. Default page size is 10.")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    static async Task<Ok<GetProductsResponse>> HandleAsync([AsParameters] GetProductsRequestParameters parameters)
    {
        var (sender, ct, pageNumber, pageSize) = parameters;

        var query = GetProducts.Of(pageNumber, pageSize);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new GetProductsResponse(result.Products, result.PageSize, result.PageCount, result.TotalCount)
        );
    }
}

public sealed record GetProductsRequestParameters(
    ISender Sender,
    CancellationToken CancellationToken,
    [FromQuery] int PageNumber = 1,
    [FromQuery] int PageSize = 10
);

public sealed record GetProductsResponse(
    IReadOnlyCollection<ProductDto> Products,
    int PageSize,
    int PageCount,
    int TotalCount
);
