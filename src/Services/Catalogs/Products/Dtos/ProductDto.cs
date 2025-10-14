namespace GenAIEshop.Catalogs.Products.Dtos;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, bool IsAvailable, string ImageUrl);
