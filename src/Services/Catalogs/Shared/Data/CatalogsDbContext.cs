using GenAIEshop.Catalogs.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Shared.Data;

public class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogsDbContext).Assembly);
    }
}
