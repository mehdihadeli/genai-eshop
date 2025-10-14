using BuildingBlocks.Extensions;
using GenAIEshop.Orders.Orders.Dtos;
using GenAIEshop.Orders.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Orders.Orders.Features.GettingOrder;

public sealed record GetOrder(Guid OrderId, Guid UserId) : IQuery<GetOrderResult>
{
    public static GetOrder Of(Guid orderId, Guid userId)
    {
        userId.NotBeEmpty();
        orderId.NotBeEmpty();

        return new GetOrder(orderId, userId);
    }
}

public sealed class GetOrderHandler(OrdersDbContext dbContext, ILogger<GetOrderHandler> logger)
    : IQueryHandler<GetOrder, GetOrderResult>
{
    public async ValueTask<GetOrderResult> Handle(GetOrder query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching order {OrderId} for user {UserId}", query.OrderId, query.UserId);

        var order = await dbContext
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId && o.UserId == query.UserId, cancellationToken);

        if (order == null)
            return new GetOrderResult(null);

        return new GetOrderResult(order.ToDto());
    }
}

public sealed record GetOrderResult(OrderDto? Order);
