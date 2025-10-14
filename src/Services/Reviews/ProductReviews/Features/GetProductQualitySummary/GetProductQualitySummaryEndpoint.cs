using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.GetProductQualitySummary;

public static class GetProductQualitySummaryEndpoint
{
    public static RouteHandlerBuilder MapGetProductQualitySummaryEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{productId:guid}/quality-summary", HandleAsync)
            .WithName(nameof(GetProductQualitySummary))
            .WithDisplayName("Get Product Quality Summary")
            .WithSummary("Retrieves AI-generated quality summary for a product.")
            .WithDescription(
                "Provides a concise quality classification and summary based on customer reviews analysis."
            )
            .Produces<GetProductQualitySummaryResponse>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest);
    }

    static async Task<Results<Ok<GetProductQualitySummaryResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetProductQualitySummaryRequestParameters parameters
    )
    {
        var (productId, sender, ct) = parameters;

        var query = GetProductQualitySummary.Of(productId);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new GetProductQualitySummaryResponse(result.ProductId, result.QualitySummary, result.GeneratedAt)
        );
    }
}

public sealed record GetProductQualitySummaryRequestParameters(
    [FromRoute] Guid ProductId,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record GetProductQualitySummaryResponse(Guid ProductId, string QualitySummary, DateTime GeneratedAt);
