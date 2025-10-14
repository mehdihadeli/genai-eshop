using GenAIEshop.Reviews.ProductReviews.Dtos;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;

public static class GetReviewsByProductEndpoint
{
    public static RouteHandlerBuilder MapGetReviewsByProductEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/{productId:guid}", HandleAsync)
            .WithName(nameof(GetReviewsByProduct))
            .WithDisplayName("Get Product Reviews")
            .WithSummary("Retrieves paginated reviews for a product.")
            .WithDescription("Returns a paginated list of reviews for the specified product, ordered by most recent.")
            .Produces<GetReviewsByProductResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    static async Task<Ok<GetReviewsByProductResponse>> HandleAsync(
        [AsParameters] GetReviewsByProductRequestParameters parameters
    )
    {
        var (sender, ct, productId, pageNumber, pageSize) = parameters;

        var query = GetReviewsByProduct.Of(productId, pageNumber, pageSize);
        var result = await sender.Send(query, ct);

        return TypedResults.Ok(
            new GetReviewsByProductResponse(result.Reviews, result.PageSize, result.PageNumber, result.TotalCount)
        );
    }
}

public sealed record GetReviewsByProductRequestParameters(
    ISender Sender,
    CancellationToken CancellationToken,
    [FromRoute] Guid ProductId,
    [FromQuery] int PageNumber = 1,
    [FromQuery] int PageSize = 10
);

public sealed record GetReviewsByProductResponse(
    IReadOnlyCollection<ReviewDto> Reviews,
    int PageSize,
    int PageNumber,
    int TotalCount
)
{
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
