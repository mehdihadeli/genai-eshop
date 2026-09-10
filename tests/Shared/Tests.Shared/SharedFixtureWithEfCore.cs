using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GenAIEshop.Tests.Shared;

public class SharedFixtureWithEfCore<TEntryPoint, TContext> : SharedFixture<TEntryPoint>
    where TEntryPoint : class
    where TContext : DbContext
{
    public PostgresContainerFixture PostgresContainerFixture { get; } = new();
    public string ConnectionString => PostgresContainerFixture.ConnectionString;

    public override async ValueTask InitializeAsync()
    {
        await PostgresContainerFixture.InitializeAsync();
        AddConfiguration("ConnectionStrings:pg-catalogsdb", ConnectionString);
        AddConfiguration("ConnectionStrings:pg-ordersdb", ConnectionString);
        AddConfiguration("ConnectionStrings:pg-reviewsdb", ConnectionString);
        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await PostgresContainerFixture.DisposeAsync();
    }

    public TContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TContext>().UseNpgsql(ConnectionString).Options;
        return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
    }

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var db = CreateDbContext();
        await PostgresContainerFixture.EnsureCreatedAsync(db, cancellationToken);
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken = default) =>
        PostgresContainerFixture.ResetAsync(cancellationToken);
}
