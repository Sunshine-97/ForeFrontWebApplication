using ForeFrontWebApplication.Data;
using ForeFrontWebApplication.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Repositories.Warehouse;

public sealed class EfWarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _db;

    public EfWarehouseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrderEntity>> GetDeliveredOrdersAsync(
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
