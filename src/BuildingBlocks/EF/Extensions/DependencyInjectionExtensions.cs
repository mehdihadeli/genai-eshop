using System.Reflection;
using BuildingBlocks.EF.Interceptors;
using BuildingBlocks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.EF.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddPostgresDbContext<TDbContext>(
        this IHostApplicationBuilder builder,
        string? connectionStringName,
        Assembly? migrationAssembly = null,
        Action<IHostApplicationBuilder>? action = null,
        Action<DbContextOptionsBuilder>? dbContextBuilder = null,
        Action<PostgresOptions>? configurator = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Add an option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configurator);

        var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();

        // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:pg-catalogsdb` key on configuration.
        // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections,
        // so environment configuration reads its data from the ` ConnectionStrings__pg-catalogsdb ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
        // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#use-a-connection-string
        // first read from aspire injected ConnectionString then read from config
        var connectionString =
            !string.IsNullOrWhiteSpace(connectionStringName)
            && !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(connectionStringName))
                ? builder.Configuration.GetConnectionString(connectionStringName)
                : postgresOptions.ConnectionString
                    ?? throw new InvalidOperationException(
                        $"Postgres connection string '{connectionStringName}' or `postgresOptions.ConnectionString` not found."
                    );

        builder.Services.AddScoped<IConnectionFactory>(sp => new NpgsqlConnectionFactory(connectionString));

        // to handle dependency injection in the interceptors, we add them to the service collection, then resolve them and add to `options.AddInterceptors`
        builder.Services.AddScoped<IInterceptor, DomainEventPublisherInterceptor>();
        builder.Services.AddSingleton<IInterceptor, AuditInterceptor>();
        builder.Services.AddSingleton<IInterceptor, SoftDeleteInterceptor>();
        builder.Services.AddSingleton<IInterceptor, GuidIdGeneratorInterceptor>();

        builder.Services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#pending-model-changes
                // https://github.com/dotnet/efcore/issues/35158
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

                var interceptors = sp.GetServices<IInterceptor>().ToList();
                if (interceptors.Count != 0)
                {
                    options.AddInterceptors(interceptors);
                }

                options
                    .UseNpgsql(
                        connectionString,
                        sqlOptions =>
                        {
                            var name =
                                migrationAssembly?.GetName().Name
                                ?? postgresOptions.MigrationAssembly
                                ?? typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(name);
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    // https://github.com/efcore/EFCore.NamingConventions
                    .UseSnakeCaseNamingConvention();

                // ref: https://andrewlock.net/series/using-strongly-typed-entity-ids-to-avoid-primitive-obsession/
                options.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<Guid>>();

                dbContextBuilder?.Invoke(options);
            }
        );

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#enrich-an-npgsql-database-context
        // For config health check and instrumentation for postgres dbcontext
        builder.EnrichNpgsqlDbContext<TDbContext>();

        action?.Invoke(builder);

        return builder;
    }
}
