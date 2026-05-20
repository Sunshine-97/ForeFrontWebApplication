using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForeFrontWebApplication.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.OrderId);

        builder.Property(o => o.OrderId)
               .HasColumnName("order_id")
               .IsRequired();

        builder.Property(o => o.KundId)
               .HasColumnName("kund_id")
               .IsRequired();

        builder.Property(o => o.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(o => o.Created)
               .HasColumnName("created")
               .IsRequired();

        builder.HasMany(o => o.Produkter)
               .WithOne()
               .HasForeignKey(l => l.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}