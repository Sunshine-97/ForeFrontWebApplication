using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Repositories.WarehouseRepo
{
  public interface IWarehouseRepository
  {
    Task<IReadOnlyList<Orders>> GetDeliveredOrdersAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
  }
}
