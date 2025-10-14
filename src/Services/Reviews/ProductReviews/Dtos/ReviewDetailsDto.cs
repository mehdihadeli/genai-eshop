namespace GenAIEshop.Reviews.ProductReviews.Dtos;

public record ReviewDetailsDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Description,
    Guid UserId,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);
