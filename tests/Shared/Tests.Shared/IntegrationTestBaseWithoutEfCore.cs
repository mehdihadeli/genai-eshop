using Xunit;

namespace GenAIEshop.Tests.Shared;

public abstract class IntegrationTestBase<TEntryPoint>(SharedFixture<TEntryPoint> sharedFixture) : IAsyncLifetime
    where TEntryPoint : class
{
    protected SharedFixture<TEntryPoint> SharedFixture { get; } = sharedFixture;
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
