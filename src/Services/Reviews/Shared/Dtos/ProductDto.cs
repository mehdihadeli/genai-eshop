namespace GenAIEshop.Reviews.Shared.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    bool IsAvailable,
    string? Description,
    string? ImageUrl = null
);
