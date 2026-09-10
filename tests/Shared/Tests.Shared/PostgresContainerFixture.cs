using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace GenAIEshop.Tests.Shared;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("genai_eshop_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithImage("postgres:17")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var checkpoint = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres }
        );

        await checkpoint.ResetAsync(connection);
    }

    public async Task EnsureCreatedAsync<TContext>(TContext dbContext, CancellationToken cancellationToken = default)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
