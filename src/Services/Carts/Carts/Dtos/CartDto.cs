namespace GenAIEshop.Carts.Carts.Dtos;

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
