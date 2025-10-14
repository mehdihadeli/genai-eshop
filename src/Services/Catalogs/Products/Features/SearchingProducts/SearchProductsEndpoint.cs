using GenAIEshop.Catalogs.Products.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.SearchingProducts;

public static class SearchProductsEndpoint
{
    public static RouteHandlerBuilder MapSearchProductsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/search", HandleAsync)
            .WithName(nameof(SearchProducts))
            .WithDisplayName("Search Products")
            .WithSummary("Searches products by name/description with pagination.")
            .WithDescription(
                "Supports regular text search or semantic search. Returns paginated results with metadata."
            )
            .Produces<SearchProductsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    static async Task<Ok<SearchProductsResponse>> HandleAsync([AsParameters] SearchProductsRequestParameters parameters)
    {
        var (sender, ct, searchTerm, keywords, searchType, pageNumber, pageSize, threshold) = parameters;

        var query = SearchProducts.Of(searchTerm, keywords, searchType, pageNumber, pageSize);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new SearchProductsResponse(
                result.Products,
                result.AIExplanationMessage,
                result.SearchType,
                result.PageSize,
                result.PageCount,
                result.TotalCount
            )
        );
    }
}

public sealed record SearchProductsRequestParameters(
    ISender Sender,
    CancellationToken CancellationToken,
    [FromQuery] string SearchTerm,
    [FromQuery] string[] Keywords,
    [FromQuery] SearchType SearchType = SearchType.Regular,
    [FromQuery] int PageNumber = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] double? Threshold = null
);

public sealed record SearchProductsResponse(
    IReadOnlyCollection<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int PageCount,
    int TotalCount
);
