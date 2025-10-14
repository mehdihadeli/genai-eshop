using System.Collections.Immutable;
using BuildingBlocks.Types;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Mediator;

public static class Extensions
{
    public static async Task DispatchDomainEventsAsync(this IPublisher publisher, DbContext ctx)
    {
        var domainEntities = ctx
            .ChangeTracker.Entries<Aggregate>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToImmutableList();

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToImmutableList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}
