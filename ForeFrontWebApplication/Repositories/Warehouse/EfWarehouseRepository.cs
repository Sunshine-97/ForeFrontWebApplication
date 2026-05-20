using ForeFrontWebApplication.Data;
using ForeFrontWebApplication.Models.Order;
using Microsoft.EntityFrameworkCore;
using OrderModel = ForeFrontWebApplication.Models.Order.Order;

namespace ForeFrontWebApplication.Repositories.Warehouse;

/// <summary>
/// EF Core + PostgreSQL implementation of <see cref="IWarehouseRepository"/>.
/// Pushes status and date filtering directly into the SQL query — no full table scan.
/// Registered as Scoped in Program.cs (lifetime matches AppDbContext).
/// </summary>
public sealed class EfWarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _db;

    public EfWarehouseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrderModel>> GetDeliveredOrdersAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default)
    {
        return await _db.Orders
            .Include(o => o.Produkter)
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered)
            .Where(o => from == null || o.Created >= from)
            .Where(o => to   == null || o.Created <= to)
            .ToListAsync(ct);
    }
}
