using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.CompareProductsByReviews;

public static class CompareProductsByReviewsEndpoint
{
    public static RouteHandlerBuilder MapCompareProductsByReviewsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("compare", HandleAsync)
            .WithName(nameof(CompareProductsByReviews))
            .WithDisplayName("Compare Products by Reviews")
            .WithSummary("Compares multiple products based on their reviews.")
            .WithDescription(
                "Performs AI-powered comparative analysis of products using customer reviews, ratings, and feedback."
            )
            .Produces<CompareProductsByReviewsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<CompareProductsByReviewsResponse>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] CompareProductsByReviewsRequestParameters parameters
    )
    {
        var (request, sender, ct) = parameters;

        var query = CompareProductsByReviews.Of(request.ProductIds.ToArray());
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new CompareProductsByReviewsResponse(result.ProductIds, result.ComparisonAnalysis, result.GeneratedAt)
        );
    }
}

public sealed record CompareProductsByReviewsRequestParameters(
    [FromBody] CompareProductsByReviewsRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record CompareProductsByReviewsRequest(List<Guid> ProductIds);

public sealed record CompareProductsByReviewsResponse(
    List<Guid> ProductIds,
    string ComparisonAnalysis,
    DateTime GeneratedAt
);
