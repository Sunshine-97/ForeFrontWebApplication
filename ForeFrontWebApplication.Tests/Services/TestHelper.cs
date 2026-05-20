using System.Text.Json;
using System.Text.Json.Serialization;
using ForeFrontWebApplication.Models.Order;

namespace ForeFrontWebApplication.Tests.Services;

internal static class TestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Reads <c>samples/testdata.json</c> and returns the orders it contains as domain objects.
    /// Order IDs, statuses and created dates are preserved exactly as declared in the file.
    /// </summary>
    public static IList<Order> LoadOrders()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "samples", "testdata.json");
        var json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<TestDataRoot>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize test data from '{path}'.");

        return root.Orders.Select(MapOrder).ToList();
    }

    private static Order MapOrder(TestOrderDto dto) => new()
    {
        OrderId   = dto.OrderId,
        KundId    = dto.Kund.Email,
        Status    = ParseStatus(dto.Status),
        Created   = DateTime.Parse(dto.Datum),
        Produkter = dto.Produkter.Select(p => MapProduct(p, dto.OrderId)).ToList(),
    };

    private static OrderLine MapProduct(TestProduktDto dto, string orderId) => new()
    {
        OrderLineId = Guid.NewGuid().ToString(),
        OrderId     = orderId,
        ProduktId   = dto.ProductId,
        Namn        = dto.Namn,
        Antal       = dto.Antal,
        Pris        = dto.Pris,
    };

    private static OrderStatus ParseStatus(string status) => status.ToLowerInvariant() switch
    {
        "pending"   => OrderStatus.Pending,
        "confirmed" => OrderStatus.Confirmed,
        "shipped"   => OrderStatus.Shipped,
        "delivered" => OrderStatus.Delivered,
        "cancelled" => OrderStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown order status: '{status}'."),
    };

    private sealed record TestDataRoot(
        [property: JsonPropertyName("orders")] IReadOnlyList<TestOrderDto> Orders);

    private sealed record TestOrderDto(
        string OrderId,
        TestKundDto Kund,
        string Status,
        string Datum,
        IReadOnlyList<TestProduktDto> Produkter);

    private sealed record TestKundDto(string Namn, string Email);

    private sealed record TestProduktDto(
        string ProductId,
        string Namn,
        int Antal,
        decimal Pris);
}
