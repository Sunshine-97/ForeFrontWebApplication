using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Repositories.Order;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderEntity>> GetAllAsync(CancellationToken ct = default);
    Task<OrderEntity?> GetByIdAsync(string orderId, CancellationToken ct = default);
    Task AddAsync(OrderEntity order, CancellationToken ct = default);
    Task UpdateAsync(OrderEntity order, CancellationToken ct = default);
    Task<bool> DeleteAsync(string orderId, CancellationToken ct = default);
}
