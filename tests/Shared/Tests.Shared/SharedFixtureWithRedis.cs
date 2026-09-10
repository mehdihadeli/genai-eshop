namespace GenAIEshop.Tests.Shared;

public sealed class SharedFixtureWithRedis<TEntryPoint> : SharedFixture<TEntryPoint>
    where TEntryPoint : class
{
    public RedisContainerFixture RedisContainerFixture { get; } = new();

    public string ConnectionString => RedisContainerFixture.ConnectionString;

    public override async ValueTask InitializeAsync()
    {
        await RedisContainerFixture.InitializeAsync();
        AddConfiguration("ConnectionStrings:redis", ConnectionString);
        AddConfiguration("CacheOptions:RedisDistributedCacheOptions:ConnectionString", ConnectionString);
        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await RedisContainerFixture.DisposeAsync();
    }
}
