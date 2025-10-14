using BuildingBlocks.EF;
using GenAIEshop.Catalogs.Products.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenAIEshop.Catalogs.Products.Data.Configurations;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(nameof(Product).Pluralize().Underscore(), nameof(Catalogs).Underscore());
        builder.HasKey(e => e.Id);

        // We'll generate GUIDs in code
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();

        // Postgres specific concurrency control
        // https://dateo-software.de/blog/concurrency-entity-framework
        // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#native-database-generated-concurrency-tokens
        // https://www.npgsql.org/efcore/modeling/concurrency.html?tabs=fluent-api
        builder.Property(p => p.Version).IsRowVersion();

        builder.Property(ci => ci.Name).HasMaxLength(DataSchemaLength.Large).IsRequired();
        builder.Property(ci => ci.Description).HasMaxLength(DataSchemaLength.SuperLarge);
        builder.Property(ci => ci.Price).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(ci => ci.IsAvailable).HasDefaultValue(true);
        builder.Property(ci => ci.ImageUrl).HasMaxLength(DataSchemaLength.ExtraLarge);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
