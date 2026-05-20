using ForeFrontWebApplication.Models.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ForeFrontWebApplication.Models.Product;

namespace ForeFrontWebApplication.Data.Configurations;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(l => l.OrderLineId);

        builder.Property(l => l.OrderLineId)
               .HasColumnName("order_line_id")
               .IsRequired();

        builder.Property(l => l.OrderId)
               .HasColumnName("order_id")
               .IsRequired();

        builder.Property(l => l.ProductId)
               .HasColumnName("product_id")
               .IsRequired();

        builder.Property(l => l.Namn)
               .HasColumnName("namn")
               .IsRequired();

        builder.Property(l => l.Pris)
               .HasColumnName("pris")
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(l => l.Antal)
               .HasColumnName("antal")
               .IsRequired();

        // FK ? products: RESTRICT prevents deleting a product used in an order
        builder.HasOne<Products>()
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
