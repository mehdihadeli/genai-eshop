using GenAIEshop.Orders.Orders.Models;

namespace GenAIEshop.Orders.Orders.Dtos;

public record OrderDto(
    Guid Id,
    Guid UserId,
    DateTime OrderDate,
    OrderStatus Status,
    string ShippingAddress,
    DateTime CreatedAt,
    Guid CreatedBy,
    DateTime? LastModifiedAt,
    Guid? LastModifiedBy,
    uint Version,
    List<OrderItemDto> Items
)
{
    public decimal TotalPrice => Items.Sum(item => item.Quantity * item.UnitPrice);
}
