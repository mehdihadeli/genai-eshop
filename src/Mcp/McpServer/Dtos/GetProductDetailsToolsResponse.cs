namespace McpServer.Shared.Dtos;

public record GetProductDetailsToolsResponse(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    string ImageUrl
);
