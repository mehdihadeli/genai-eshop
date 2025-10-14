using BuildingBlocks.Mediator;
using Mediator;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.EF.Interceptors;

public class DomainEventPublisherInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            publisher.DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            await publisher.DispatchDomainEventsAsync(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
