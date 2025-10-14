namespace McpServer.Shared.Dtos;

public record SemanticSearchProductsToolResponse(
    IReadOnlyCollection<ProductDto> Products,
    string AIExplanationMessage,
    SearchType SearchType,
    int PageSize,
    int PageCount,
    int TotalCount
);
