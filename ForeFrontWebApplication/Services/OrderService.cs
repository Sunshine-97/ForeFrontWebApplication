using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories.Customer;
using ForeFrontWebApplication.Repositories.Order;
using ForeFrontWebApplication.Repositories.Product;

namespace ForeFrontWebApplication.Services
{
    public sealed class OrderService : IOrderService
    {
        private static readonly OrderStatus[] StatusFlow =
            [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled];

        private readonly IOrderRepository    _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository  _productRepository;

        public OrderService(
            IOrderRepository    orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository  productRepository)
        {
            _orderRepository    = orderRepository;
            _customerRepository = customerRepository;
            _productRepository  = productRepository;
        }

        public async Task<IReadOnlyList<OrderEntity>> GetAllAsync(CancellationToken ct = default) =>
            await _orderRepository.GetAllAsync(ct);

        public async Task<OrderEntity?> GetByIdAsync(string orderId, CancellationToken ct = default) =>
            await _orderRepository.GetByIdAsync(orderId, ct);

        public async Task<OrderResponse> CreateAsync(OrderRequest req, CancellationToken ct = default)
        {
            // Validate customer exists before touching order_lines
            var customerExists = await _customerRepository.ExistsAsync(req.KundId, ct);
            if (!customerExists)
                throw new KeyNotFoundException($"Customer '{req.KundId}' does not exist.");

            var orderId   = Guid.NewGuid().ToString();
            var produkter = await MapToOrderLinesAsync(req.Produkter, orderId, ct);

            var order = new OrderEntity
            {
                OrderId   = orderId,
                KundId    = req.KundId,
                Produkter = produkter,
                Status    = OrderStatus.Pending,
                Created   = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order, ct);

            return new OrderResponse
            {
                OrderId = order.OrderId,
                Status  = order.Status
            };
        }

        public async Task<OrderEntity?> UpdateStatusAsync(string orderId, OrderStatus newStatus, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);
            if (order is null)
                return null;

            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException(
                    $"Cannot transition from {order.Status} to {newStatus}.");

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order, ct);
            return order;
        }

        public async Task<bool> DeleteAsync(string orderId, CancellationToken ct = default) =>
            await _orderRepository.DeleteAsync(orderId, ct);

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

        private async Task<IList<OrderLine>> MapToOrderLinesAsync(
            List<OrderItemRequest> items,
            string orderId,
            CancellationToken ct)
        {
            var lines = new List<OrderLine>(items.Count);

            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProduktId, ct)
                    ?? throw new KeyNotFoundException($"Product '{item.ProduktId}' does not exist.");

                lines.Add(new OrderLine
                {
                    OrderLineId = Guid.NewGuid().ToString(),
                    OrderId     = orderId,
                    ProductId   = product.ProductId,
                    Namn        = product.Namn,
                    Pris        = product.Pris,
                    Antal       = item.Antal
                });
            }

            return lines;
        }
    }
}
