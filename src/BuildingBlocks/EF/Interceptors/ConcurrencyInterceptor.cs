using BuildingBlocks.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.EF.Interceptors;

/// <summary>
/// Use for application managed concurrency token, and it should use with `IsConcurrencyToken = true`, which use for application level concurrency token handling
/// </summary>
public class ConcurrencyInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<AuditableEntity>())
        {
            // https://dateo-software.de/blog/concurrency-entity-framework
            // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#application-managed-concurrency-tokens
            // application managed concurrency token, and it should use with `IsConcurrencyToken = true` which use for application level concurrency token handling
            switch (entry.State)
            {
                case EntityState.Modified:
                case EntityState.Added:
                    // entry.Entity.Version = Guid.CreateVersion7();
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
