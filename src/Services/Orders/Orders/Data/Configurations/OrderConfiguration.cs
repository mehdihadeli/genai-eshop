using BuildingBlocks.EF;
using GenAIEshop.Orders.Orders.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenAIEshop.Orders.Orders.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(nameof(Order).Pluralize().Underscore(), nameof(Orders).Underscore());

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired().ValueGeneratedNever();
        builder.Property(p => p.Version).IsRowVersion();

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.OrderDate).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().IsRequired();
        builder.Property(e => e.ShippingAddress).HasMaxLength(DataSchemaLength.SuperLarge).IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.OrderDate);
        builder.HasIndex(e => e.Status);

        builder.OwnsMany(
            e => e.Items,
            ownedBuilder =>
            {
                ownedBuilder.ToTable(nameof(OrderItem).Pluralize().Underscore(), nameof(Orders).Underscore());

                // ownedBuilder.WithOwner().HasForeignKey("OrderId"); // Shadow FK
                //
                // // Composite PK: (OrderId, ProductId)
                // ownedBuilder.HasKey("OrderId", nameof(OrderItem.ProductId));

                ownedBuilder.Property(e => e.ProductId).IsRequired();
                ownedBuilder.Property(e => e.ProductName).HasMaxLength(DataSchemaLength.Large).IsRequired();
                ownedBuilder.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)").IsRequired();
                ownedBuilder.Property(e => e.Quantity).IsRequired();

                ownedBuilder.HasIndex(e => e.ProductId);
            }
        );

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
