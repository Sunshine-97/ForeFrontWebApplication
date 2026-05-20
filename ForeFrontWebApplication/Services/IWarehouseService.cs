using ForeFrontWebApplication.Models.Warehouse;

namespace ForeFrontWebApplication.Services;

public interface IWarehouseService
{
    Task<IReadOnlyList<OrderVolumes>> GetVolumesAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<OrderVolumes>> GetTopProductsAsync(CancellationToken ct = default);
}
