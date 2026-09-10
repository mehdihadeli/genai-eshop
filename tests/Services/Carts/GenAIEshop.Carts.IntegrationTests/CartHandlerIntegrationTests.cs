using GenAIEshop.Carts.Carts.Features.GettingCart;
using GenAIEshop.Tests.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace GenAIEshop.Carts.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class CartHandlerIntegrationTests(SharedFixture<Program> sharedFixture)
    : IntegrationTestBase<Program>(sharedFixture)
{
    [Fact]
    public async Task Get_cart_returns_empty_result_when_cache_has_no_cart()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var handler = new GetCartHandler(provider.GetRequiredService<HybridCache>());

        var result = await handler.Handle(GetCart.Of(Guid.NewGuid()), CancellationToken.None);

        result.Cart.ShouldBeNull();
    }
}
