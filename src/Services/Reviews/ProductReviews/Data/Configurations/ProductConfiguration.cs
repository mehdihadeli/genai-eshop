using BuildingBlocks.EF;
using GenAIEshop.Reviews.ProductReviews.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenAIEshop.Reviews.ProductReviews.Data.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        // Table name and schema
        builder.ToTable(nameof(ProductReview).Pluralize().Underscore(), nameof(Review).Pluralize().Underscore());
        builder.HasKey(e => e.Id);

        // We'll generate GUIDs in code
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();

        // Postgres specific concurrency control
        // https://dateo-software.de/blog/concurrency-entity-framework
        // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#native-database-generated-concurrency-tokens
        // https://www.npgsql.org/efcore/modeling/concurrency.html?tabs=fluent-api
        builder.Property(p => p.Version).IsRowVersion();

        builder.Property(r => r.ProductId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(DataSchemaLength.SuperLarge);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
