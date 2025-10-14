using McpServer.Shared.Clients;
using McpServer.Shared.Dtos;

namespace McpServer.Shared.Contracts;

public interface ICatalogServiceClient
{
    Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetProductsByIdAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken = default
    );

    Task<SearchProductsResponse> SearchProductsAsync(
        string searchTerm,
        double threshold,
        SearchType searchType = SearchType.Regular,
        string[]? keywords = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );
}
