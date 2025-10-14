using McpServer.Shared.Dtos;

namespace McpServer.Shared.Clients;

public record SearchProductsResponse(
    List<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int PageCount,
    int TotalCount
);
