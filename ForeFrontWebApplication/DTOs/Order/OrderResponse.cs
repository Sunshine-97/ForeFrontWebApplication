using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.DTOs.Order
{
    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal TotalPris { get; set; } = 0m;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}
