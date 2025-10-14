using BuildingBlocks.Types;

namespace GenAIEshop.Carts.Carts.Models;

public class Cart : AuditableEntity
{
    public required Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(item => item.Quantity * item.UnitPrice);
}
