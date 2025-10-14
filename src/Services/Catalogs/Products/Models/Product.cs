using BuildingBlocks.Types;

namespace GenAIEshop.Catalogs.Products.Models;

public class Product : AuditableEntity, ISoftDelete
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public required string ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
