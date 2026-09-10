using System.Net.Http.Json;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Recommendation.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public sealed class RecommendationApiTests(SharedFixture<Program> sharedFixture)
    : EndToEndTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Recommendation_endpoint_accepts_openai_compatible_configuration()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/recommendations/recommend",
            new { query = "laptop for software development" }
        );

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
