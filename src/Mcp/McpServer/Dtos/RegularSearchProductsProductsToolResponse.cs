namespace McpServer.Shared.Dtos;

public record RegularSearchProductsProductsToolResponse(
    IReadOnlyCollection<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int PageCount,
    int TotalCount
);