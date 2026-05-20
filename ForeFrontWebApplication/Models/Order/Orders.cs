namespace ForeFrontWebApplication.Models.Order
{
    public class OrderEntity
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public required string KundId { get; set; }
        public IList<OrderLine> Produkter { get; set; } = [];
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
