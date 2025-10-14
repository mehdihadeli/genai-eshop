using GenAIEshop.Carts.Carts.Dtos;

namespace GenAIEshop.Carts.Carts.Contracts;

public interface ICartsService
{
    Task<CartDto?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(
        Guid userId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default
    );
    Task<CartDto> UpdateItemQuantityAsync(
        Guid userId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default
    );
    Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
