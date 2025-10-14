using System.ComponentModel;
using System.Text.Json;
using BuildingBlocks.Serialization;
using McpServer.Shared.Contracts;
using McpServer.Shared.Dtos;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public class ProductServiceTools(ICatalogServiceClient catalogServiceClient)
{
    [
        McpServerTool(Name = "HybridSearchProducts"),
        Description(
            "Performs a hybrid products search combining semantic understanding with keyword matching for the most comprehensive results using vector database full-text search. Use this for complex queries where you want both conceptual understanding and exact keyword matching. Returns products with AI-powered insights."
        )
    ]
    public async Task<HybridSearchProductsProductsProductsToolResponse> HybridSearchProducts(
        [Description("Search query describing for searching in products")] string query,
        [Description("Specific keywords to help refine the products search")] string[]? keywords = null
    )
    {
        var response = await catalogServiceClient.SearchProductsAsync(
            searchTerm: query,
            // for more results, increase the threshold
            threshold: 0.6,
            searchType: SearchType.Hybrid,
            keywords: keywords,
            pageNumber: 1,
            pageSize: int.MaxValue
        );

        var result = new HybridSearchProductsProductsProductsToolResponse(response.Products);

        return result;
    }
}
