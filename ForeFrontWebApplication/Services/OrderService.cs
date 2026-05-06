using System.Collections.Concurrent;
using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

public class OrderService : IOrderService
{
    private static readonly OrderStatus[] StatusFlow =
        [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered];

    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public IReadOnlyList<Order> GetAll()
    {
        return _orders.Values.ToList().AsReadOnly();
    }

    public Order? GetById(string orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return order;
    }

    public Order Create(Order order)
    {
        order.OrderId = Guid.NewGuid().ToString();
        order.Status = OrderStatus.Pending;
        order.Datum = DateTime.UtcNow;
        _orders[order.OrderId] = order;
        return order;
    }

    public Order? UpdateStatus(string orderId, OrderStatus newStatus)
    {
        if (!_orders.TryGetValue(orderId, out var order))
            return null;

        if (!IsValidTransition(order.Status, newStatus))
            throw new InvalidOperationException(
                $"Cannot transition from {order.Status} to {newStatus}.");

        order.Status = newStatus;
        return order;
    }

    public bool Delete(string orderId)
    {
        return _orders.TryRemove(orderId, out _);
    }

    private static bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        var currentIndex = Array.IndexOf(StatusFlow, current);
        var nextIndex = Array.IndexOf(StatusFlow, next);
        return nextIndex == currentIndex + 1;
    }
}
