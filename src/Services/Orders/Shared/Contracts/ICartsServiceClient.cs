using GenAIEshop.Orders.Shared.Dtos;

namespace GenAIEshop.Orders.Shared.Contracts;

public interface ICartsServiceClient
{
    Task<CartDto?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
