using System.Net;
using GenAIEshop.Reviews.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Reviews.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public sealed class ReviewsApiTests(SharedFixtureWithEfCore<Program, ReviewsDbContext> sharedFixture)
    : EndToEndTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Reviews_endpoint_returns_a_valid_response_for_unknown_product()
    {
        using var response = await Client.GetAsync($"/api/v1/reviews/{Guid.NewGuid()}?PageNumber=1&PageSize=5");

        (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound).ShouldBeTrue();
    }

    [Fact]
    public async Task Reviews_a2a_agent_card_is_discoverable()
    {
        using var response = await Client.GetAsync("/reviews/.well-known/agent-card.json");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("ReviewsAgent");
    }
}
