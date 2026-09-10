using GenAIEshop.Orders.Orders.Features.GettingOrder;

namespace GenAIEshop.Orders.UnitTests;

public sealed class OrderQueryTests
{
    [Fact]
    public void Of_preserves_order_and_user_ids()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var query = GetOrder.Of(orderId, userId);

        query.OrderId.ShouldBe(orderId);
        query.UserId.ShouldBe(userId);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Of_rejects_empty_ids(bool emptyOrderId, bool emptyUserId)
    {
        var orderId = emptyOrderId ? Guid.Empty : Guid.NewGuid();
        var userId = emptyUserId ? Guid.Empty : Guid.NewGuid();

        Should.Throw<BuildingBlocks.Exceptions.ValidationException>(() => GetOrder.Of(orderId, userId));
    }
}
