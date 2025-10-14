using GenAIEshop.Orders.Shared.Dtos;

namespace GenAIEshop.Orders.Shared.Contracts;

public interface ICatalogServiceClient
{
    Task<CatalogProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
