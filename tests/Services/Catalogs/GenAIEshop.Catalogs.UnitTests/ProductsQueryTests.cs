using GenAIEshop.Catalogs.Products.Features.GettingProducts;

namespace GenAIEshop.Catalogs.UnitTests;

public sealed class ProductsQueryTests
{
    [Fact]
    public void Of_creates_query_with_expected_paging()
    {
        var query = GetProducts.Of(2, 5);

        query.PageNumber.ShouldBe(2);
        query.PageSize.ShouldBe(5);
        query.Skip.ShouldBe(5);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public void Of_rejects_invalid_paging(int pageNumber, int pageSize)
    {
        Should.Throw<BuildingBlocks.Exceptions.ValidationException>(() => GetProducts.Of(pageNumber, pageSize));
    }
}
