using Xunit;

namespace GenAIEshop.Tests.Shared;

public abstract class EndToEndTestBase<TEntryPoint>(SharedFixture<TEntryPoint> sharedFixture) : IAsyncLifetime
    where TEntryPoint : class
{
    protected SharedFixture<TEntryPoint> SharedFixture { get; } = sharedFixture;
    protected HttpClient Client => SharedFixture.Client;

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
