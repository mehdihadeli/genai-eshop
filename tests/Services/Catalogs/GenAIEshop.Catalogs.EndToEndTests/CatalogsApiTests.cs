using System.Net;
using GenAIEshop.Catalogs.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Catalogs.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public sealed class CatalogsApiTests(SharedFixtureWithEfCore<Program, CatalogsDbContext> sharedFixture)
    : EndToEndTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Products_endpoint_returns_a_valid_response()
    {
        using var response = await Client.GetAsync("/api/v1/products?PageNumber=1&PageSize=5");

        (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound).ShouldBeTrue();
    }
}
