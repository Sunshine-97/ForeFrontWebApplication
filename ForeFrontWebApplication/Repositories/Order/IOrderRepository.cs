using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Repositories.Order;

public interface IOrderRepository
{
    Task<IReadOnlyList<Models.Order.Order>> GetAllAsync(CancellationToken ct = default);
    Task<Models.Order.Order?> GetByIdAsync(string orderId, CancellationToken ct = default);
    Task AddAsync(Models.Order.Order order, CancellationToken ct = default);
    Task<bool> DeleteAsync(string orderId, CancellationToken ct = default);
}
