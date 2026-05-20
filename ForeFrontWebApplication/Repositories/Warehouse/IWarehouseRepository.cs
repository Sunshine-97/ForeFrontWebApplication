namespace ForeFrontWebApplication.Repositories.Warehouse;

public interface IWarehouseRepository
{
    /// <summary>
    /// Returns all delivered orders, optionally filtered by creation date range.
    /// In the EF implementation this filter is pushed into the SQL query.
    /// </summary>
    Task<IReadOnlyList<Models.Order.Order>> GetDeliveredOrdersAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default);
}