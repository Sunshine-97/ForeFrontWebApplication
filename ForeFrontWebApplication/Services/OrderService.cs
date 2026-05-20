using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories.Order;

namespace ForeFrontWebApplication.Services
{
    public sealed class OrderService : IOrderService
    {
        private static readonly OrderStatus[] StatusFlow =
            [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled];

        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default) =>
            await _repository.GetAllAsync(ct);

        public async Task<Order?> GetByIdAsync(string orderId, CancellationToken ct = default) =>
            await _repository.GetByIdAsync(orderId, ct);

        public async Task<OrderResponse> CreateAsync(OrderRequest req, CancellationToken ct = default)
        {
            var orderId   = Guid.NewGuid().ToString();
            var produkter = MapToProducts(req.Produkter, orderId);

            var order = new Order
            {
                OrderId   = orderId,
                KundId    = req.KundId,
                Produkter = produkter,
                Status    = OrderStatus.Pending,
                Created   = DateTime.UtcNow
            };

            await _repository.AddAsync(order, ct);

            return new OrderResponse
            {
                OrderId = order.OrderId,
                Status  = order.Status
            };
        }

        public async Task<Order?> UpdateStatusAsync(string orderId, OrderStatus newStatus, CancellationToken ct = default)
        {
            var order = await _repository.GetByIdAsync(orderId, ct);
            if (order is null)
                return null;

            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException(
                    $"Cannot transition from {order.Status} to {newStatus}.");

            order.Status = newStatus;
            return order;
        }

        public async Task<bool> DeleteAsync(string orderId, CancellationToken ct = default) =>
            await _repository.DeleteAsync(orderId, ct);

        private static bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            if (current == OrderStatus.Delivered || current == OrderStatus.Cancelled)
                return false;

            if (next == OrderStatus.Cancelled)
                return true;

            var currentIndex = Array.IndexOf(StatusFlow, current);
            var nextIndex    = Array.IndexOf(StatusFlow, next);
            return nextIndex == currentIndex + 1;
        }

        private static IList<OrderLine> MapToProducts(List<OrderItemRequest> produkter, string orderId)
        {
            return produkter.Select(dto => new OrderLine
            {
                OrderLineId = Guid.NewGuid().ToString(),
                OrderId     = orderId,
                ProduktId   = dto.ProduktId,
                Namn        = $"Product {dto.ProduktId}", // Placeholder — replace with catalogue lookup
                Antal       = dto.Antal,
                Pris        = 10.0m                       // Placeholder — replace with catalogue lookup
            }).ToList();
        }
    }
}
