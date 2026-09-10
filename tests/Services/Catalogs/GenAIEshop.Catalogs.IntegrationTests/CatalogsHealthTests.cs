using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Catalogs.IntegrationTests;

public sealed class CatalogsHealthTests
{
    [Fact]
    public async Task Health_endpoint_is_available()
    {
        using var client = TestHostConfiguration.CreateServiceClient("catalogs");

        using var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
    }
}
