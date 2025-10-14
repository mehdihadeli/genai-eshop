namespace GenAIEshop.Carts.Carts.Models;

public class CartItem
{
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Quantity { get; set; }
}
