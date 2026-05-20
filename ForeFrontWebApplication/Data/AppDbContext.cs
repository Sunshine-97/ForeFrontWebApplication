using ForeFrontWebApplication.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order>     Orders     { get; init; } = null!;
    public DbSet<OrderLine> OrderLines { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
