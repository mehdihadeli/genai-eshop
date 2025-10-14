using BuildingBlocks.Extensions;
using GenAIEshop.Carts.Carts.Dtos;
using GenAIEshop.Carts.Carts.Models;
using GenAIEshop.Carts.Shared.Constants;
using Mediator;
using Microsoft.Extensions.Caching.Hybrid;

namespace GenAIEshop.Carts.Carts.Features.GettingCart;

public sealed record GetCart(Guid UserId) : IQuery<GetCartResult>
{
    public static GetCart Of(Guid userId)
    {
        return new GetCart(userId.NotBeEmpty());
    }
}

public sealed class GetCartHandler(HybridCache hybridCache) : IQueryHandler<GetCart, GetCartResult>
{
    public async ValueTask<GetCartResult> Handle(GetCart query, CancellationToken cancellationToken)
    {
        var cart = await hybridCache.GetOrCreateAsync(
            key: CacheKeys.GetCartKey(query.UserId),
            factory: _ => ValueTask.FromResult<Cart?>(null),
            options: new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
                Expiration = TimeSpan.FromDays(7),
            },
            cancellationToken: cancellationToken
        );

        return new GetCartResult(cart?.ToDto());
    }
}

public sealed record GetCartResult(CartDto? Cart);
