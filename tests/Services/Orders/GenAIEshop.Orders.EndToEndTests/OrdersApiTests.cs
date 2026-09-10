using System.Net;
using GenAIEshop.Orders.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Orders.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public sealed class OrdersApiTests(SharedFixtureWithEfCoreAndRedis<Program, OrdersDbContext> sharedFixture)
    : EndToEndTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Order_endpoint_returns_a_valid_response_for_unknown_order()
    {
        using var response = await Client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}?UserId={Guid.NewGuid()}");

        (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound).ShouldBeTrue();
    }
}
