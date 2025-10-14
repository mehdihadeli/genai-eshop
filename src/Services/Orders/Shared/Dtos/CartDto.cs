namespace GenAIEshop.Orders.Shared.Dtos;

public record CartDto(
    Guid Id,
    Guid UserId,
    DateTime CreatedAt,
    Guid CreatedBy,
    DateTime? LastModifiedAt,
    Guid? LastModifiedBy,
    List<CartItemDto> Items
)
{
    public decimal TotalPrice => Items.Sum(item => item.Quantity * item.UnitPrice);
}

public record CartItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
