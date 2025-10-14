using BuildingBlocks.AI.AgentFramework;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.AnalyzeProductReviews;

public static class AnalyzeProductReviewsEndpoint
{
    public static RouteHandlerBuilder MapAnalyzeProductReviewsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/{productId:guid}/analyze", HandleAsync)
            .WithName(nameof(AnalyzeProductReviews))
            .WithDisplayName("Analyze Product Reviews")
            .WithSummary("Performs AI-powered analysis of product reviews.")
            .WithDescription(
                "Uses AI to analyze all reviews for a product, providing sentiment analysis, quality assessment, and key insights."
            )
            .Produces<AnalyzeProductReviewsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<AnalyzeProductReviewsResponse>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] AnalyzeProductReviewsRequestParameters parameters
    )
    {
        var (productId, sender, ct, agentOrchestrationType) = parameters;

        var command = AnalyzeProductReviews.Of(productId, agentOrchestrationType);
        var result = await sender.Send(command, ct);

        return TypedResults.Ok(
            new AnalyzeProductReviewsResponse(result.ProductId, result.Analysis, result.GeneratedAt)
        );
    }
}

public sealed record AnalyzeProductReviewsRequestParameters(
    [FromRoute] Guid ProductId,
    ISender Sender,
    CancellationToken CancellationToken,
    [FromQuery] AgentOrchestrationType AgentOrchestrationType = AgentOrchestrationType.Normal
);

public sealed record AnalyzeProductReviewsResponse(Guid ProductId, string Analysis, DateTime GeneratedAt);
