using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.ProductReviews.Dtos;
using GenAIEshop.Reviews.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;

public sealed record GetReviewsByProduct(Guid ProductId, int PageNumber, int PageSize)
    : IQuery<GetReviewsByProductResult>
{
    public static GetReviewsByProduct Of(Guid productId, int pageNumber = 1, int pageSize = 10)
    {
        productId.NotBeEmpty();
        return new GetReviewsByProduct(productId, pageNumber.NotBeNegativeOrZero(), pageSize.NotBeNegativeOrZero());
    }

    public int Skip => (PageNumber - 1) * PageSize;
}

public sealed class GetReviewsByProductHandler(ReviewsDbContext dbContext, ILogger<GetReviewsByProductHandler> logger)
    : IQueryHandler<GetReviewsByProduct, GetReviewsByProductResult>
{
    public async ValueTask<GetReviewsByProductResult> Handle(GetReviewsByProduct query, CancellationToken ct)
    {
        logger.LogInformation("Fetching reviews for product {ProductId}", query.ProductId);

        var totalCount = await dbContext.ProductReviews.CountAsync(
            r => r.ProductId == query.ProductId && !r.IsDeleted,
            cancellationToken: ct
        );

        var reviews = await dbContext
            .ProductReviews.Where(r => r.ProductId == query.ProductId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(r => new ReviewDto(r.Id, r.ProductId, r.UserId, r.Rating, r.Comment, r.CreatedAt))
            .ToListAsync(ct);

        return new GetReviewsByProductResult(reviews.AsReadOnly(), query.PageSize, query.PageNumber, totalCount);
    }
}

public sealed record GetReviewsByProductResult(
    IReadOnlyCollection<ReviewDto> Reviews,
    int PageSize,
    int PageNumber,
    int TotalCount
)
{
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
