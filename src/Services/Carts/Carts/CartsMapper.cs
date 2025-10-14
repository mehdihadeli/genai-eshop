using GenAIEshop.Carts.Carts.Dtos;
using GenAIEshop.Carts.Carts.Models;

namespace GenAIEshop.Carts.Carts;

public static class CartsMapper
{
    public static CartDto ToDto(this Cart cart)
    {
        return new CartDto(
            Id: cart.Id,
            UserId: cart.UserId,
            CreatedAt: cart.CreatedAt,
            CreatedBy: cart.CreatedBy,
            Items: cart.Items.Select(ToDto).ToList(),
            LastModifiedAt: cart.LastModifiedAt,
            LastModifiedBy: cart.LastModifiedBy
        );
    }

    public static CartItemDto ToDto(this CartItem item)
    {
        return new CartItemDto(
            ProductId: item.ProductId,
            ProductName: item.ProductName,
            UnitPrice: item.UnitPrice,
            Quantity: item.Quantity
        );
    }

    public static Cart ToDomain(this CartDto dto)
    {
        return new Cart
        {
            Id = dto.Id,
            UserId = dto.UserId,
            CreatedAt = dto.CreatedAt,
            CreatedBy = dto.CreatedBy,
            Items = dto.Items.Select(ToDomain).ToList(),
            LastModifiedAt = dto.LastModifiedAt,
            LastModifiedBy = dto.LastModifiedBy,
        };
    }

    public static CartItem ToDomain(this CartItemDto dto)
    {
        return new CartItem
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            UnitPrice = dto.UnitPrice,
            Quantity = dto.Quantity,
        };
    }
}
