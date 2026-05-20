using ForeFrontWebApplication.Data;
using Microsoft.EntityFrameworkCore;
using ForeFrontWebApplication.Models.Product;

namespace ForeFrontWebApplication.Repositories.Product;

public sealed class EfProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public EfProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Products> GetByIdAsync(string productId, CancellationToken ct = default) =>
        await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId, ct);

    public async Task<IReadOnlyList<Products>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Products.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Products product, CancellationToken ct = default)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }
}
