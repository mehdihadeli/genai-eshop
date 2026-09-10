using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Carts.IntegrationTests;

public sealed class CartsHealthTests
{
    [Fact]
    public async Task Health_endpoint_is_available()
    {
        using var client = TestHostConfiguration.CreateServiceClient("carts");

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
