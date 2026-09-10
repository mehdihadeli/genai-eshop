using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Reviews.IntegrationTests;

public sealed class ReviewsHealthTests
{
    [Fact]
    public async Task Health_endpoint_is_available()
    {
        using var client = TestHostConfiguration.CreateServiceClient("reviews");

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
