using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.AnalyzeReviewTrends;

public static class AnalyzeReviewTrendsEndpoint
{
    public static RouteHandlerBuilder MapAnalyzeReviewTrendsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{productId:guid}/trends", HandleAsync)
            .WithName(nameof(AnalyzeReviewTrends))
            .WithDisplayName("Analyze Review Trends")
            .WithSummary("Analyzes review trends and patterns over time.")
            .WithDescription(
                "Identifies changes in sentiment, common themes, and patterns in product reviews over a specified period."
            )
            .Produces<AnalyzeReviewTrendsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest);
    }

    static async Task<Results<Ok<AnalyzeReviewTrendsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] AnalyzeReviewTrendsRequestParameters parameters
    )
    {
        var (productId, sender, ct, daysBack) = parameters;

        var query = AnalyzeReviewTrends.Of(productId, daysBack);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new AnalyzeReviewTrendsResponse(
                result.ProductId,
                result.TrendsAnalysis,
                result.PeriodInDays,
                result.GeneratedAt
            )
        );
    }
}

public sealed record AnalyzeReviewTrendsRequestParameters(
    [FromRoute] Guid ProductId,
    ISender Sender,
    CancellationToken CancellationToken,
    [FromQuery] int DaysBack = 30
);

public sealed record AnalyzeReviewTrendsResponse(
    Guid ProductId,
    string TrendsAnalysis,
    int PeriodInDays,
    DateTime GeneratedAt
);
