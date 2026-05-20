using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
    Task<Order?> GetByIdAsync(string orderId, CancellationToken ct = default);
    Task<OrderResponse> CreateAsync(OrderRequest request, CancellationToken ct = default);
    Task<Order?> UpdateStatusAsync(string orderId, OrderStatus newStatus, CancellationToken ct = default);
    Task<bool> DeleteAsync(string orderId, CancellationToken ct = default);
}
