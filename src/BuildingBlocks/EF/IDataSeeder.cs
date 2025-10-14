using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.EF;

public interface IDataSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
