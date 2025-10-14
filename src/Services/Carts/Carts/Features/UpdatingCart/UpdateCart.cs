using BuildingBlocks.Extensions;
using GenAIEshop.Carts.Carts.Dtos;
using GenAIEshop.Carts.Shared.Constants;
using GenAIEshop.Carts.Shared.Contracts;
using Mediator;
using Microsoft.Extensions.Caching.Hybrid;

namespace GenAIEshop.Carts.Carts.Features.UpdatingCart;

public sealed record UpdateCart(Guid UserId, List<CartItemChange> Changes) : ICommand<UpdateCartResult>
{
    public static UpdateCart Of(Guid userId, List<CartItemChange>? changes)
    {
        userId.NotBeEmpty();
        changes.NotBeNull();

        foreach (var change in changes)
        {
            change.ProductId.NotBeEmpty();
            change.Quantity.NotBeNegativeOrZero();
        }

        return new UpdateCart(userId, changes);
    }
}

public sealed class UpdateCartHandler(
    HybridCache hybridCache,
    ICatalogServiceClient catalogClient,
    ILogger<UpdateCartHandler> logger
) : ICommandHandler<UpdateCart, UpdateCartResult>
{
    public async ValueTask<UpdateCartResult> Handle(UpdateCart command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Updating cart for user {UserId} with {ChangeCount} changes",
            command.UserId,
            command.Changes.Count
        );

        var cartKey = CacheKeys.GetCartKey(command.UserId);

        // Get existing cart or create new one
        var cart = await hybridCache.GetOrCreateAsync(
            key: cartKey,
            factory: _ => ValueTask.FromResult<CartDto?>(null),
            options: new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
                Expiration = TimeSpan.FromDays(7),
            },
            cancellationToken: cancellationToken
        );

        if (cart == null)
        {
            cart = new CartDto(
                Id: Guid.NewGuid(),
                UserId: command.UserId,
                CreatedAt: DateTime.UtcNow,
                CreatedBy: command.UserId,
                LastModifiedAt: null,
                LastModifiedBy: null,
                Items: new List<CartItemDto>()
            );
        }

        var currentItems = cart.Items.ToList();

        // Process each change
        foreach (var change in command.Changes)
        {
            if (change.Quantity <= 0)
            {
                // Remove item
                currentItems.RemoveAll(i => i.ProductId == change.ProductId);
            }
            else
            {
                // Add or update item
                var existingItem = currentItems.FirstOrDefault(i => i.ProductId == change.ProductId);

                if (existingItem != null)
                {
                    // Update quantity
                    currentItems = currentItems
                        .Select(i => i.ProductId == change.ProductId ? i with { Quantity = change.Quantity } : i)
                        .ToList();
                }
                else
                {
                    // Add new item - fetch product info from catalog
                    var product =
                        await catalogClient.GetProductByIdAsync(change.ProductId, cancellationToken)
                        ?? throw new InvalidOperationException($"Product {change.ProductId} not found in catalog.");

                    if (!product.IsAvailable)
                        throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

                    currentItems.Add(
                        new CartItemDto(
                            ProductId: change.ProductId,
                            ProductName: product.Name,
                            UnitPrice: product.Price,
                            Quantity: change.Quantity
                        )
                    );
                }
            }
        }

        // If cart is empty, delete it
        if (currentItems.Count == 0)
        {
            await hybridCache.RemoveAsync(cartKey, cancellationToken);
            return new UpdateCartResult(cart with { Items = currentItems });
        }

        // Update cart with new items
        var updatedCart = cart with
        {
            Items = currentItems,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = command.UserId,
        };

        await hybridCache.SetAsync(
            key: cartKey,
            value: updatedCart,
            options: new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
                Expiration = TimeSpan.FromDays(7),
            },
            cancellationToken: cancellationToken
        );

        return new UpdateCartResult(updatedCart);
    }
}

public sealed record UpdateCartResult(CartDto Cart);
