using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Repositories.OrderRepo
{

  public interface IOrderRepository
  {
    Task<IReadOnlyList<Orders>> GetAllAsync(CancellationToken ct = default);
    Task<Orders?> GetByIdAsync(string orderId, CancellationToken ct = default);
    Task AddAsync(Orders order, CancellationToken ct = default);
    Task UpdateAsync(Orders order, CancellationToken ct = default);
    Task<bool> DeleteAsync(string orderId, CancellationToken ct = default);
  }
}
