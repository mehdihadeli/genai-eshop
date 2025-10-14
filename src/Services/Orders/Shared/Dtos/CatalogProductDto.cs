namespace GenAIEshop.Orders.Shared.Dtos;

public record CatalogProductDto(Guid Id, string Name, decimal Price, bool IsAvailable, string? ImageUrl = null);
