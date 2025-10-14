using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Recommendation.Recommendations.Features.ComparingProducts;

public static class CompareProductsEndpoint
{
    public static RouteHandlerBuilder MapCompareProductsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/compare", HandleAsync)
            .WithName(nameof(CompareProducts))
            .WithDisplayName("Compare Products")
            .WithSummary("Compares multiple products using AI-powered analysis.")
            .WithDescription(
                "Uses AI to compare multiple products based on features, pricing, customer reviews, and specifications to help with purchase decisions."
            )
            .Produces<CompareProductsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<CompareProductsResponse>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] CompareProductsRequestParameters parameters
    )
    {
        var (request, sender, ct) = parameters;

        var command = CompareProducts.Of(request.ProductIds, request.Criteria);
        var result = await sender.Send(command, ct);

        return TypedResults.Ok(new CompareProductsResponse(result.Comparison, result.GeneratedAt));
    }
}

public sealed record CompareProductsRequestParameters(
    [FromBody] CompareProductsRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record CompareProductsRequest(Guid[] ProductIds, string? Criteria = null);

public sealed record CompareProductsResponse(string Comparison, DateTime GeneratedAt);
