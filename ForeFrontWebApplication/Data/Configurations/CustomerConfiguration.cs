using ForeFrontWebApplication.Models.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForeFrontWebApplication.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customers>
{
    public void Configure(EntityTypeBuilder<Customers> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerId)
               .HasColumnName("customer_id")
               .IsRequired();

        builder.Property(c => c.Namn)
               .HasColumnName("namn")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(c => c.Email)
               .HasColumnName("email")
               .HasMaxLength(200)
               .IsRequired();

        builder.HasIndex(c => c.Email)
               .IsUnique();
    }
}
