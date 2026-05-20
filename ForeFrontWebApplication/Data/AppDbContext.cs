using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Customer;
using Microsoft.EntityFrameworkCore;
using ForeFrontWebApplication.Models.Product;

namespace ForeFrontWebApplication.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customers>     Customers  { get; init; } = null!;
    public DbSet<Products> Products   { get; init; } = null!;
    public DbSet<OrderEntity>  Orders     { get; init; } = null!;
    public DbSet<OrderLine>    OrderLines { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
