using ForeFrontWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ForeFrontWebApplication.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
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
