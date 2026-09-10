using System.Net;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Carts.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public sealed class CartsApiTests(SharedFixtureWithRedis<Program> sharedFixture)
    : EndToEndTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Cart_endpoint_returns_a_valid_response_for_unknown_user()
    {
        using var response = await Client.GetAsync($"/api/v1/carts?UserId={Guid.NewGuid()}");

        (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound).ShouldBeTrue();
    }
}
