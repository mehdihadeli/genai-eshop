using BuildingBlocks.Extensions;
using GenAIEshop.Carts.Shared.Constants;
using Mediator;
using Microsoft.Extensions.Caching.Hybrid;
using ICommand = Mediator.ICommand;

namespace GenAIEshop.Carts.Carts.Features.ClearingCart;

public sealed record ClearCart(Guid UserId) : ICommand
{
    public static ClearCart Of(Guid userId)
    {
        return new ClearCart(userId.NotBeEmpty());
    }
}

public sealed class ClearCartHandler(HybridCache hybridCache) : ICommandHandler<ClearCart, Unit>
{
    public async ValueTask<Unit> Handle(ClearCart command, CancellationToken ct)
    {
        var cartKey = CacheKeys.GetCartKey(command.UserId);
        await hybridCache.RemoveAsync(cartKey, ct);

        return Unit.Value;
    }
}
