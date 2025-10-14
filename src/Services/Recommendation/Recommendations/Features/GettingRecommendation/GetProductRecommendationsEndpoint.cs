using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Recommendation.Recommendations.Features.GettingRecommendation;

public static class GetProductRecommendationsEndpoint
{
    public static RouteHandlerBuilder MapGetProductRecommendationsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/recommend", HandleAsync)
            .WithName(nameof(GetProductRecommendations))
            .WithDisplayName("Get Product Recommendations")
            .WithSummary("Generates AI-powered product recommendations based on user query.")
            .WithDescription(
                "Uses AI to provide personalized product recommendations by combining search capabilities with comprehensive review analysis."
            )
            .Produces<GetProductRecommendationsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<Ok<GetProductRecommendationsResponse>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] GetProductRecommendationsRequestParameters parameters
    )
    {
        var (request, sender, ct) = parameters;

        var command = GetProductRecommendations.Of(request.Query);
        var result = await sender.Send(command, ct);

        return TypedResults.Ok(new GetProductRecommendationsResponse(result.Recommendations, result.GeneratedAt));
    }
}

public sealed record GetProductRecommendationsRequestParameters(
    [FromBody] GetProductRecommendationsRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record GetProductRecommendationsRequest(string Query);

public sealed record GetProductRecommendationsResponse(string Recommendations, DateTime GeneratedAt);
