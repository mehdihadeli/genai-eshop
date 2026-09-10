using GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;
using GenAIEshop.Reviews.ProductReviews.Models;
using GenAIEshop.Reviews.Shared.Data;
using GenAIEshop.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenAIEshop.Reviews.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ReviewsHandlerIntegrationTests(SharedFixtureWithEfCore<Program, ReviewsDbContext> sharedFixture)
    : IntegrationTestBase<Program, ReviewsDbContext>(sharedFixture)
{
    [Fact]
    public async Task Get_reviews_excludes_deleted_reviews_and_applies_paging()
    {
        await using var db = SharedFixture.CreateDbContext();
        await SharedFixture.EnsureDatabaseCreatedAsync();

        var productId = Guid.NewGuid();
        db.ProductReviews.AddRange(
            new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                UserId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Visible",
                CreatedAt = DateTime.UtcNow,
            },
            new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                UserId = Guid.NewGuid(),
                Rating = 1,
                Comment = "Deleted",
                IsDeleted = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            }
        );
        await db.SaveChangesAsync();

        var result = await new GetReviewsByProductHandler(db, NullLogger<GetReviewsByProductHandler>.Instance).Handle(
            GetReviewsByProduct.Of(productId, 1, 10),
            CancellationToken.None
        );

        result.TotalCount.ShouldBe(1);
        result.Reviews.Count.ShouldBe(1);
        result.Reviews.Single().Comment.ShouldBe("Visible");
    }
}
