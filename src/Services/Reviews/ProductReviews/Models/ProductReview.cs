using BuildingBlocks.Types;

namespace GenAIEshop.Reviews.ProductReviews.Models;

public class ProductReview : AuditableEntity, ISoftDelete
{
    public required Guid ProductId { get; set; }
    public required Guid UserId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
