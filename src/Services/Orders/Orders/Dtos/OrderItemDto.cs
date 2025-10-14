namespace GenAIEshop.Orders.Orders.Dtos;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
