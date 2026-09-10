using GenAIEshop.Recommendation.Recommendations.Features.GettingRecommendation;

namespace GenAIEshop.Recommendation.UnitTests;

public sealed class RecommendationQueryTests
{
    [Fact]
    public void Of_preserves_query()
    {
        GetProductRecommendations.Of("wireless headphones").Query.ShouldBe("wireless headphones");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Of_rejects_blank_query(string query)
    {
        Should.Throw<BuildingBlocks.Exceptions.ValidationException>(() => GetProductRecommendations.Of(query));
    }
}
