using ForeFrontWebApplication.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForeFrontWebApplication.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Products>
{
    public void Configure(EntityTypeBuilder<Products> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductId)
               .HasColumnName("product_id")
               .IsRequired();

        builder.Property(p => p.Namn)
               .HasColumnName("namn")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Pris)
               .HasColumnName("pris")
               .HasColumnType("numeric(18,2)")
               .IsRequired();
    }
}
