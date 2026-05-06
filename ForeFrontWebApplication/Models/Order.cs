namespace ForeFrontWebApplication.Models;

public class Order
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public required Customer Kund { get; set; }
    public List<Product> Produkter { get; set; } = [];
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime Datum { get; set; } = DateTime.UtcNow;
}
