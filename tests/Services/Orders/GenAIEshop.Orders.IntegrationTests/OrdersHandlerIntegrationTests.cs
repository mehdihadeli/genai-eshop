using GenAIEshop.Orders.Orders.Features.GettingOrder;
using GenAIEshop.Orders.Orders.Models;
using GenAIEshop.Orders.Shared.Data;
using GenAIEshop.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenAIEshop.Orders.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrdersHandlerIntegrationTests(SharedFixtureWithEfCore<Program, OrdersDbContext> sharedFixture)
    : IntegrationTestBase<Program, OrdersDbContext>(sharedFixture)
{
    [Fact]
    public async Task Get_order_returns_only_order_owned_by_requested_user()
    {
        await using var db = SharedFixture.CreateDbContext();
        await SharedFixture.EnsureDatabaseCreatedAsync();

        var ownerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        db.Orders.Add(
            new Order
            {
                Id = orderId,
                UserId = ownerId,
                Status = OrderStatus.Pending,
                ShippingAddress = "1 Test Street",
            }
        );
        await db.SaveChangesAsync();

        var result = await new GetOrderHandler(db, NullLogger<GetOrderHandler>.Instance).Handle(
            GetOrder.Of(orderId, ownerId),
            CancellationToken.None
        );

        result.Order.ShouldNotBeNull();
        result.Order!.Id.ShouldBe(orderId);

        var unauthorized = await new GetOrderHandler(db, NullLogger<GetOrderHandler>.Instance).Handle(
            GetOrder.Of(orderId, Guid.NewGuid()),
            CancellationToken.None
        );

        unauthorized.Order.ShouldBeNull();
    }
}
