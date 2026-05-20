using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Repositories.Warehouse;

public interface IWarehouseRepository
{
    Task<IReadOnlyList<OrderEntity>> GetDeliveredOrdersAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default);
}