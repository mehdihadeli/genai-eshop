using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GenAIEshop.Tests.Shared;

public abstract class IntegrationTestBase<TEntryPoint, TContext>(
    SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture
) : IAsyncLifetime
    where TEntryPoint : class
    where TContext : DbContext
{
    protected SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; } = sharedFixture;
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public virtual async ValueTask InitializeAsync()
    {
        await SharedFixture.EnsureDatabaseCreatedAsync(CancellationToken);
        await SharedFixture.ResetDatabaseAsync(CancellationToken);
    }

    public virtual ValueTask DisposeAsync() => new(SharedFixture.ResetDatabaseAsync(CancellationToken));
}
