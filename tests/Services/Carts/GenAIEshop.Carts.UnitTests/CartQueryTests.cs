using GenAIEshop.Carts.Carts.Features.GettingCart;

namespace GenAIEshop.Carts.UnitTests;

public sealed class CartQueryTests
{
    [Fact]
    public void Of_preserves_user_id()
    {
        var userId = Guid.NewGuid();

        GetCart.Of(userId).UserId.ShouldBe(userId);
    }

    [Fact]
    public void Of_rejects_empty_user_id()
    {
        Should.Throw<BuildingBlocks.Exceptions.ValidationException>(() => GetCart.Of(Guid.Empty));
    }
}
