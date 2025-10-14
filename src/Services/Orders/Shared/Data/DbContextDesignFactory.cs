using BuildingBlocks.EF;
using BuildingBlocks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Environments = BuildingBlocks.Types.Environments;

namespace GenAIEshop.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
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

        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>()
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

        return (OrdersDbContext)Activator.CreateInstance(typeof(OrdersDbContext), optionsBuilder.Options);
    }
}
