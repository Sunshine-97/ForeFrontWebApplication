using ForeFrontWebApplication.Data;
using ForeFrontWebApplication.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Repositories.OrderRepo
{
  /// <summary>
  /// EF Core + PostgreSQL implementation of <see cref="IOrderRepository"/>.
  /// Registered as Scoped in Program.cs (lifetime matches AppDbContext).
  /// </summary>
  public sealed class OrderRepository : IOrderRepository
  {
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
      _db = db;
    }

    public async Task<IReadOnlyList<Orders>> GetAllAsync(CancellationToken ct = default)
    {
      return await _db.Orders
          .Include(o => o.Produkter)
          .AsNoTracking()
          .ToListAsync(ct);
    }

    public async Task<Orders?> GetByIdAsync(string orderId, CancellationToken ct = default)
    {
      return await _db.Orders
          .Include(o => o.Produkter)
          .AsNoTracking()
          .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
    }

    public async Task AddAsync(Orders order, CancellationToken ct = default)
    {
      await _db.Orders.AddAsync(order, ct);
      await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Orders order, CancellationToken ct = default)
    {
      _db.Orders.Update(order);
      await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(string orderId, CancellationToken ct = default)
    {
      var order = await _db.Orders.FindAsync([orderId], ct);
      if (order is null)
        return false;

      _db.Orders.Remove(order);
      await _db.SaveChangesAsync(ct);
      return true;
    }
  }
}
