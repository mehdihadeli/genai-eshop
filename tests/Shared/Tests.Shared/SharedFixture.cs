using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GenAIEshop.Tests.Shared;

public class SharedFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly List<KeyValuePair<string, string?>> _configuration = [];

    public CustomWebApplicationFactory<TEntryPoint> Factory { get; private set; } = null!;
    public IServiceProvider Services => Factory.Services;
    public HttpClient Client => Factory.CreateTestClient();

    public void AddConfiguration(string key, string? value) => _configuration.Add(new(key, value));

    public virtual async ValueTask InitializeAsync()
    {
        Factory = new CustomWebApplicationFactory<TEntryPoint>();
        foreach (var pair in _configuration)
            Factory.WithConfiguration(pair.Key, pair.Value);

        await Task.CompletedTask;
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }
}
