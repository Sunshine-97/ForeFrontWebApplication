using ForeFrontWebApplication.Commands;
using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orders,
    ICustomerRepository customers,
    IProductRepository products)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand req, CancellationToken ct)
    {
        if (!await customers.ExistsAsync(req.KundId, ct))
            throw new KeyNotFoundException($"Customer '{req.KundId}' not found.");

        var orderId = Guid.NewGuid().ToString();
        var lines   = new List<OrderLine>(req.Produkter.Count);
        var total   = 0m;

        foreach (var item in req.Produkter)
        {
            var product = await products.GetByIdAsync(item.ProduktId, ct)
                ?? throw new KeyNotFoundException($"Product '{item.ProduktId}' not found.");

            lines.Add(new OrderLine
            {
                OrderLineId = Guid.NewGuid().ToString(),
                OrderId     = orderId,
                ProductId   = product.ProductId,
                Namn        = product.Namn,
                Pris        = product.Pris,
                Antal       = item.Antal,
            });

            total += product.Pris * item.Antal;
        }

        var order = new Orders
        {
            OrderId  = orderId,
            KundId   = req.KundId,
            Produkter = lines,
            Status   = OrderStatus.Pending,
            Created  = DateTime.UtcNow,
        };

        await orders.AddAsync(order, ct);

        return new OrderResponse
        {
            OrderId   = order.OrderId,
            TotalPris = total,
            Status    = order.Status,
        };
    }
}
