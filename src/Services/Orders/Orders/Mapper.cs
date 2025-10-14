using GenAIEshop.Orders.Orders.Dtos;
using GenAIEshop.Orders.Orders.Models;

namespace GenAIEshop.Orders.Orders;

public static class Mapper
{
    public static OrderDto ToDto(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));

        return new OrderDto(
            Id: order.Id,
            UserId: order.UserId,
            OrderDate: order.OrderDate,
            Status: order.Status,
            ShippingAddress: order.ShippingAddress,
            CreatedAt: order.CreatedAt,
            CreatedBy: order.CreatedBy,
            LastModifiedAt: order.LastModifiedAt,
            LastModifiedBy: order.LastModifiedBy,
            Version: order.Version,
            Items: order
                .Items.Select(item => new OrderItemDto(
                    ProductId: item.ProductId,
                    ProductName: item.ProductName,
                    UnitPrice: item.UnitPrice,
                    Quantity: item.Quantity
                ))
                .ToList()
        );
    }
}
