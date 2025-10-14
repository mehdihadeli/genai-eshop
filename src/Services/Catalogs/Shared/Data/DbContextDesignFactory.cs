using BuildingBlocks.EF;
using BuildingBlocks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.

namespace GenAIEshop.Catalogs.Shared.Data;

public class CatalogsDbContextDesignFactory : IDesignTimeDbContextFactory<CatalogsDbContext>
{
    public CatalogsDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var postgresOptions = configuration.BindOptions<PostgresOptions>();

        var connectionString = postgresOptions.ConnectionString;
        Console.WriteLine($"connectionString is : {connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<CatalogsDbContext>()
            .UseNpgsql(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }
            )
            .UseSnakeCaseNamingConvention();

        optionsBuilder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<Guid>>();

        return (CatalogsDbContext)Activator.CreateInstance(typeof(CatalogsDbContext), optionsBuilder.Options);
    }
}
