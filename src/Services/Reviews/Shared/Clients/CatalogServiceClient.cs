using System.Net;
using BuildingBlocks.Serialization;
using GenAIEshop.Reviews.Shared.Contracts;
using GenAIEshop.Reviews.Shared.Dtos;

namespace GenAIEshop.Reviews.Shared.Clients;

public class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger) : ICatalogServiceClient
{
    private const string BasePath = "api/v1/products";

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BasePath}/{productId}";
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var productDto = await response.Content.ReadFromJsonAsync<ProductDto>(
                options: SystemTextJsonSerializerOptions.DefaultSerializerOptions,
                cancellationToken: cancellationToken
            );
            return productDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch product {ProductId} from Catalogs service", productId);
            throw;
        }
    }

    public Task<IReadOnlyList<ProductDto>> GetProductsByIdAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(productIds);

        var ids = productIds.Distinct().ToArray();
        return ids.Length == 0 ? Task.FromResult<IReadOnlyList<ProductDto>>([]) : GetAsync(ids, cancellationToken);

        async Task<IReadOnlyList<ProductDto>> GetAsync(Guid[] idsLocal, CancellationToken ct)
        {
            try
            {
                var query = string.Join("&", idsLocal.Select(id => $"ids={Uri.EscapeDataString(id.ToString())}"));
                var url = $"{BasePath}/by-ids?{query}";

                using var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var products = await response
                    .Content.ReadFromJsonAsync<List<ProductDto>>(
                        options: SystemTextJsonSerializerOptions.DefaultSerializerOptions,
                        cancellationToken: ct
                    )
                    .ConfigureAwait(false);

                return products ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch {Count} products from Catalogs service", idsLocal.Length);
                throw;
            }
        }
    }
}
