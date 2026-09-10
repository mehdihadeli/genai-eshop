using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Orders.IntegrationTests;

public sealed class OrdersHealthTests
{
    [Fact]
    public async Task Health_endpoint_is_available()
    {
        using var client = TestHostConfiguration.CreateServiceClient("orders");

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
