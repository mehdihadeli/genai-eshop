using System.Net;
using BuildingBlocks.Serialization;
using GenAIEshop.Orders.Shared.Contracts;
using GenAIEshop.Orders.Shared.Dtos;

namespace GenAIEshop.Orders.Shared.Clients;

public class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger) : ICatalogServiceClient
{
    private const string BasePath = "api/v1/products";

    public async Task<CatalogProductDto?> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var url = $"{BasePath}/{productId}";
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var productDto = await response.Content.ReadFromJsonAsync<CatalogProductDto>(
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
}
