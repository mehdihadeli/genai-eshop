using GenAIEshop.Carts.Shared.Dtos;

namespace GenAIEshop.Carts.Shared.Contracts;

public interface ICatalogServiceClient
{
    Task<CatalogProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
