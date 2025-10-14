using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Recommendation.Recommendations.Features.GettingPersonalizedRecommendation;

public static class GetPersonalizedRecommendationsEndpoint
{
    public static RouteHandlerBuilder MapGetPersonalizedRecommendationsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/personalized-recommend", HandleAsync)
            .WithName(nameof(GetPersonalizedRecommendations))
            .WithDisplayName("Get Personalized Recommendations")
            .WithSummary("Generates personalized product recommendations based on user profile and preferences.")
            .Produces<GetPersonalizedRecommendationsResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest);
    }

    static async Task<Results<Ok<GetPersonalizedRecommendationsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetPersonalizedRecommendationsRequestParameters parameters
    )
    {
        var (request, sender, ct) = parameters;
        var command = GetPersonalizedRecommendations.Of(
            request.UserId,
            request.Query,
            request.Preferences,
            request.Category
        );
        var result = await sender.Send(command, ct);
        return TypedResults.Ok(new GetPersonalizedRecommendationsResponse(result.Recommendations, result.GeneratedAt));
    }
}

public sealed record GetPersonalizedRecommendationsRequestParameters(
    [FromBody] GetPersonalizedRecommendationsRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record GetPersonalizedRecommendationsRequest(
    Guid UserId,
    string? Query = null,
    string? Preferences = null,
    string? Category = null
);

public sealed record GetPersonalizedRecommendationsResponse(string Recommendations, DateTime GeneratedAt);
