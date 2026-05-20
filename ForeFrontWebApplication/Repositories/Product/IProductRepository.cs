using ForeFrontWebApplication.Models.Product;

namespace ForeFrontWebApplication.Repositories.Product;

public interface IProductRepository
{
    Task<Products> GetByIdAsync(string productId, CancellationToken ct = default);
    Task<IReadOnlyList<Products>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Products product, CancellationToken ct = default);
}
