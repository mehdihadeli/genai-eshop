using GenAIEshop.Reviews.ProductReviews.Dtos;
using GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.CreatingReview;

public static class CreateReviewEndpoint
{
    public static RouteHandlerBuilder MapCreateReviewEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("{productId:guid}", HandleAsync)
            .WithName(nameof(CreateReview))
            .WithDisplayName("Create Product Review")
            .WithSummary("Submits a new review and rating for a product.")
            .WithDescription("Creates a new review for the specified product with rating and optional comment.")
            .Produces<ReviewDto>(StatusCodes.Status201Created)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
    }

    static async Task<Results<CreatedAtRoute<ReviewDto>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] CreateReviewRequestParameters parameters
    )
    {
        var (productId, request, sender, ct) = parameters;

        var command = CreateReview.Of(productId, request.Rating, request.Comment);
        var result = await sender.Send(command, ct);

        return TypedResults.CreatedAtRoute(
            result.Review,
            nameof(GetReviewsByProduct),
            new { productId = result.Review.ProductId }
        );
    }
}

public sealed record CreateReviewRequestParameters(
    [FromRoute] Guid ProductId,
    [FromBody] CreateReviewRequest Request,
    ISender Sender,
    CancellationToken CancellationToken
);

public sealed record CreateReviewRequest(int Rating, string? Comment = null);
