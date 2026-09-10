using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Recommendation.IntegrationTests;

public sealed class RecommendationHealthTests
{
    [Fact]
    public async Task Health_endpoint_is_available()
    {
        using var client = TestHostConfiguration.CreateServiceClient("recommendation");

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
