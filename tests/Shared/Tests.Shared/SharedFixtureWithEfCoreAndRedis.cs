using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Tests.Shared;

public sealed class SharedFixtureWithEfCoreAndRedis<TEntryPoint, TContext>
    : SharedFixtureWithEfCore<TEntryPoint, TContext>
    where TEntryPoint : class
    where TContext : DbContext
{
    public RedisContainerFixture RedisContainerFixture { get; } = new();

    public override async ValueTask InitializeAsync()
    {
        await RedisContainerFixture.InitializeAsync();
        AddConfiguration("ConnectionStrings:redis", RedisContainerFixture.ConnectionString);
        AddConfiguration(
            "CacheOptions:RedisDistributedCacheOptions:ConnectionString",
            RedisContainerFixture.ConnectionString
        );
        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await RedisContainerFixture.DisposeAsync();
    }
}
