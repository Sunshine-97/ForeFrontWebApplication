using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

public interface IOrderService
{
    IReadOnlyList<Order> GetAll();
    Order? GetById(string orderId);
    Order Create(Order order);
    Order? UpdateStatus(string orderId, OrderStatus newStatus);
    bool Delete(string orderId);
    IReadOnlyList<OrderVolumes> GetVolumes();
    IReadOnlyList<OrderVolumes> GetVolumes(DateTime? from, DateTime? tom);
}
