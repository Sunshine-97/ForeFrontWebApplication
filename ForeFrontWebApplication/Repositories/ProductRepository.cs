using ForeFrontWebApplication.Data;
using Microsoft.EntityFrameworkCore;
using ForeFrontWebApplication.Models.Product;

<<<<<<<< HEAD:ForeFrontWebApplication/Repositories/ProductRepository.cs
namespace ForeFrontWebApplication.Repositories
========
namespace ForeFrontWebApplication.Repositories.Product;

public sealed class ProductRepository : IProductRepository
<<<<<<<< HEAD:ForeFrontWebApplication/Repositories/ProductRepository.cs
>>>>>>>> renamed repos:ForeFrontWebApplication/Repositories/Product/ProductRepository.cs
========
>>>>>>>> 62baaf8259629f29a2a0cf83b0e17191581157d8:ForeFrontWebApplication/Repositories/Product/ProductRepository.cs
{

  public sealed class ProductRepository : IProductRepository
  {
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
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
}
