namespace GenAIEshop.Carts.Carts.Dtos;

public record CartItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
