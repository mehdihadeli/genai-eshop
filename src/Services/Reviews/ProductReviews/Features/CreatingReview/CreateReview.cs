using BuildingBlocks.Exceptions;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.ProductReviews.Dtos;
using GenAIEshop.Reviews.ProductReviews.Models;
using GenAIEshop.Reviews.Shared.Contracts;
using GenAIEshop.Reviews.Shared.Data;
using Mediator;

namespace GenAIEshop.Reviews.ProductReviews.Features.CreatingReview;

public sealed record CreateReview(Guid ProductId, int Rating, string? Comment) : ICommand<CreateReviewResult>
{
    public static CreateReview Of(Guid productId, int rating, string? comment)
    {
        productId.NotBeEmpty();
        if (rating < 1 || rating > 5)
            throw new ValidationException("Rating must be between 1 and 5.");
        if (comment is { Length: > 500 })
            throw new ValidationException("Comment must be less than 500 characters.");

        return new CreateReview(productId, rating, comment);
    }
}

public sealed class CreateReviewHandler(
    ReviewsDbContext dbContext,
    ILogger<CreateReviewHandler> logger,
    ICatalogServiceClient catalogServiceClient
) : ICommandHandler<CreateReview, CreateReviewResult>
{
    public async ValueTask<CreateReviewResult> Handle(CreateReview command, CancellationToken ct)
    {
        logger.LogInformation("Creating review for product {ProductId}", command.ProductId);

        var product =
            await catalogServiceClient.GetProductByIdAsync(command.ProductId, ct)
            ?? throw new InvalidOperationException($"Product {command.ProductId} not found in catalog.");

        if (!product.IsAvailable)
            throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

        var review = new ProductReview
        {
            ProductId = command.ProductId,
            //TODO: replace with actual user ID from claims
            UserId = Guid.NewGuid(),
            Rating = command.Rating,
            Comment = command.Comment?.Trim(),
        };

        await dbContext.ProductReviews.AddAsync(review, ct);
        await dbContext.SaveChangesAsync(ct);

        var dto = new ReviewDto(
            Id: review.Id,
            ProductId: review.ProductId,
            UserId: review.UserId,
            Rating: review.Rating,
            Comment: review.Comment,
            CreatedAt: review.CreatedAt
        );

        logger.LogInformation("Reviews {ReviewId} created for product {ProductId}", review.Id, command.ProductId);

        return new CreateReviewResult(dto);
    }
}

public sealed record CreateReviewResult(ReviewDto Review);
