using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Models.Warehouse;

namespace ForeFrontWebApplication.Services;

public interface IWarehouseService
{
    /// <summary>
    /// Returns aggregated product volumes across all delivered orders,
    /// optionally filtered to orders created within the given date range.
    /// </summary>
    Task<IReadOnlyList<OrderVolumes>> GetVolumesAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the top 10 products by total sold quantity across all delivered orders.
    /// </summary>
    Task<IReadOnlyList<OrderVolumes>> GetTopProductsAsync(CancellationToken ct = default);
}
