using GenAIEshop.Reviews.Shared.Dtos;

namespace GenAIEshop.Reviews.Shared.Contracts;

public interface ICatalogServiceClient
{
    Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetProductsByIdAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken = default
    );
}
