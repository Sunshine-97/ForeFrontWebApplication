using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderEntity>> GetAllAsync(CancellationToken ct = default);
    Task<OrderEntity?> GetByIdAsync(string orderId, CancellationToken ct = default);
    Task<OrderResponse> CreateAsync(OrderRequest request, CancellationToken ct = default);
    Task<OrderEntity?> UpdateStatusAsync(string orderId, OrderStatus newStatus, CancellationToken ct = default);
    Task<bool> DeleteAsync(string orderId, CancellationToken ct = default);
}
