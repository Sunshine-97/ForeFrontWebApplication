using System.Collections.Concurrent;
using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

public class OrderService : IOrderService
{
    private static readonly OrderStatus[] StatusFlow =
        [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled];

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
        order.Status = order.Status;
        order.Created = order.Created == default ? DateTime.UtcNow : order.Created;
        _orders[order.OrderId] = order;
        return order;
    }

    public Order? UpdateStatus(string orderId, OrderStatus newStatus)
    {
        if (!_orders.TryGetValue(orderId, out var order))
            return null;

        order.Status = newStatus;
        return order;
    }

    public bool Delete(string orderId)
    {
        return _orders.TryRemove(orderId, out _);
    }

    public IReadOnlyList<OrderVolumes> GetVolumes()
    {
        return _orders.Values
            .Where(order => order.Status == OrderStatus.Delivered)
            .SelectMany(val => val.Produkter)
            .GroupBy(key => key.ProduktId)
            .Select(val => new OrderVolumes
            {
                ProduktId = val.Key,
                Namn = val.First().Namn,
                Antal = val.Sum(p => p.Antal),
                Pris = val.First().Pris
            })
            .OrderByDescending(x => x.Antal)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<OrderVolumes> GetVolumes(DateTime? from, DateTime? tom)
    {
        var query = _orders.Values
            .Where(o => o.Status == OrderStatus.Delivered);

        return _orders.Values
            .Where(order => order.Status == OrderStatus.Delivered && from < order.Created && tom > order.Created)
            .SelectMany(order => order.Produkter)
            .GroupBy(products => products.ProduktId)
            .Select(val => new OrderVolumes
            {
                ProduktId = val.Key,
                Namn = val.First().Namn,
                Antal = val.Sum(p => p.Antal),
                Pris = val.First().Pris
            })
            .OrderByDescending(x => x.Antal)
            .ToList()
            .AsReadOnly();
    }

}
