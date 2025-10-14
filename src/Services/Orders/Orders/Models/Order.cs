using BuildingBlocks.Types;

namespace GenAIEshop.Orders.Orders.Models;

public class Order : AuditableEntity, ISoftDelete
{
    public required Guid UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required string ShippingAddress { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(item => item.Quantity * item.UnitPrice);
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
