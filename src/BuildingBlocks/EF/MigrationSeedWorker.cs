using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationExtensions = BuildingBlocks.EF.Extensions.MigrationExtensions;

namespace BuildingBlocks.EF;

public class MigrationSeedWorker<TContext>(IServiceProvider serviceProvider) : IHostedService
    where TContext : DbContext
{
    private static readonly ActivitySource ActivitySource = new(MigrationExtensions.ActivitySourceName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder<TContext>>();
        var logger = scopeServiceProvider.GetRequiredService<ILogger<TContext>>();
        var context = scopeServiceProvider.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(() => ExecuteAsync(seeder, context));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name
            );

            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task ExecuteAsync<TContext>(IDataSeeder<TContext> seeder, TContext context)
        where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        await context.Database.MigrateAsync();
        await seeder.SeedAsync(context);
    }
}
