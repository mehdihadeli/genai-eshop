using GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;

namespace GenAIEshop.Reviews.UnitTests;

public sealed class ReviewsQueryTests
{
    [Fact]
    public void Of_creates_query_with_expected_paging()
    {
        var productId = Guid.NewGuid();

        var query = GetReviewsByProduct.Of(productId, 3, 4);

        query.ProductId.ShouldBe(productId);
        query.PageNumber.ShouldBe(3);
        query.PageSize.ShouldBe(4);
        query.Skip.ShouldBe(8);
    }

    [Fact]
    public void Of_rejects_empty_product_id()
    {
        Should.Throw<BuildingBlocks.Exceptions.ValidationException>(() => GetReviewsByProduct.Of(Guid.Empty));
    }
}
