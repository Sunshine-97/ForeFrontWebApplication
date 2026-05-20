using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Repositories.Warehouse;
using ForeFrontWebApplication.Services;
using NSubstitute;
using Xunit;

namespace ForeFrontWebApplication.Tests.Services;

/// <summary>
/// Tests for <see cref="WarehouseService"/>.
/// <see cref="IWarehouseRepository"/> is mocked so orders with any status and date
/// can be provided without touching the database.
/// </summary>
public class WarehouseServiceTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly WarehouseService _sut;

    public WarehouseServiceTests()
    {
        _sut = new WarehouseService(_repository);
    }

    // ?? Mocked data builders ??????????????????????????????????????????????????

    private static OrderEntity FakeOrder(
        string           kundId,
        IList<OrderLine> produkter,
        DateTime         created,
        string?          orderId = null) => new()
    {
        OrderId   = orderId ?? Guid.NewGuid().ToString(),
        KundId    = kundId,
        Produkter = produkter,
        Status    = OrderStatus.Delivered, // repository only returns delivered orders
        Created   = created
    };

    private static IList<OrderLine> Lines(params (string id, string namn, int antal, decimal pris)[] items) =>
        items.Select(p => new OrderLine
        {
            OrderLineId = Guid.NewGuid().ToString(),
            OrderId     = "order-placeholder",
            ProductId   = p.id,
            Namn        = p.namn,
            Antal       = p.antal,
            Pris        = p.pris
        }).ToList();

    /// <summary>
    /// Configures the repository mock to return the given orders for any date filter.
    /// </summary>
    private void SetupDeliveredOrders(params OrderEntity[] orders) =>
        _repository
            .GetDeliveredOrdersAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<OrderEntity>)orders.ToList());

    private void SeedFromTestData() =>
        _repository
            .GetDeliveredOrdersAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<OrderEntity>)TestHelper.LoadOrders()
                .Where(o => o.Status == OrderStatus.Delivered)
                .ToList());

    // ?? GetVolumesAsync — no date filter ?????????????????????????????????????

    [Fact]
    public async Task GetVolumes_NoDateFilter_AggregatesDeliveredOrders()
    {
        var today = DateTime.UtcNow;
        SetupDeliveredOrders(
            FakeOrder("K-001", Lines(("p1", "Skruv M6",  200, 2m)), today),
            FakeOrder("K-002", Lines(("p2", "Bult M8",    50, 5m)), today),
            FakeOrder("K-004", Lines(("p1", "Skruv M6",  150, 2m)), today)
        );

        var result = await _sut.GetVolumesAsync();

        Assert.Equal(2,          result.Count);
        Assert.Equal("Skruv M6", result[0].Namn);
        Assert.Equal(350,        result[0].Antal);
        Assert.Equal("Bult M8",  result[1].Namn);
        Assert.Equal(50,         result[1].Antal);
    }

    [Fact]
    public async Task GetVolumes_NoDateFilter_OrderedByAntalDescending()
    {
        var today = DateTime.UtcNow;
        SetupDeliveredOrders(
            FakeOrder("K-001", Lines(("p1", "A",   5, 1m)), today),
            FakeOrder("K-002", Lines(("p2", "B", 100, 1m)), today),
            FakeOrder("K-003", Lines(("p3", "C",  20, 1m)), today)
        );

        var result  = await _sut.GetVolumesAsync();
        var amounts = result.Select(v => v.Antal).ToList();

        Assert.Equal(amounts.OrderByDescending(a => a).ToList(), amounts);
    }

    [Fact]
    public async Task GetVolumes_NoDeliveredOrders_ReturnsEmptyList()
    {
        SetupDeliveredOrders();

        Assert.Empty(await _sut.GetVolumesAsync());
    }

    // ?? GetVolumesAsync — date filter delegated to repository ?????????????????

    [Fact]
    public async Task GetVolumes_WithDateRange_PassesDatesToRepository()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

        _repository.GetDeliveredOrdersAsync(from, to, Arg.Any<CancellationToken>()).Returns(
            (IReadOnlyList<OrderEntity>)new List<OrderEntity>());

        await _sut.GetVolumesAsync(from, to);

        await _repository.Received(1).GetDeliveredOrdersAsync(from, to);
    }

    [Fact]
    public async Task GetVolumes_WithDateFilter_EmptyRange_ReturnsEmpty()
    {
        _repository.GetDeliveredOrdersAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
                   .Returns((IReadOnlyList<OrderEntity>)new List<OrderEntity>());

        Assert.Empty(await _sut.GetVolumesAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-1)));
    }

    // ?? GetTopProductsAsync ???????????????????????????????????????????????????

    [Fact]
    public async Task GetTopProducts_ReturnsMaxTen()
    {
        var today = DateTime.UtcNow;
        var orders = Enumerable.Range(1, 15)
            .Select(i => FakeOrder($"K-{i:D3}", Lines(($"p{i}", $"Product {i}", i * 10, 1m)), today))
            .ToArray();
        SetupDeliveredOrders(orders);

        var result = await _sut.GetTopProductsAsync();

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task GetTopProducts_FewerThanTen_ReturnsAll()
    {
        var today = DateTime.UtcNow;
        SetupDeliveredOrders(
            FakeOrder("K-001", Lines(("p1", "A", 100, 1m)), today),
            FakeOrder("K-002", Lines(("p2", "B",  50, 1m)), today),
            FakeOrder("K-003", Lines(("p3", "C",  25, 1m)), today)
        );

        var result = await _sut.GetTopProductsAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetTopProducts_SortedByTotalSoldDescending()
    {
        var today = DateTime.UtcNow;
        SetupDeliveredOrders(
            FakeOrder("K-001", Lines(("p1", "A",   5, 1m)), today),
            FakeOrder("K-002", Lines(("p2", "B", 200, 1m)), today),
            FakeOrder("K-003", Lines(("p3", "C",  50, 1m)), today)
        );

        var result  = await _sut.GetTopProductsAsync();
        var amounts = result.Select(v => v.Antal).ToList();

        Assert.Equal(amounts.OrderByDescending(a => a).ToList(), amounts);
        Assert.Equal(200, result[0].Antal);
    }

    [Fact]
    public async Task GetTopProducts_NoDeliveredOrders_ReturnsEmptyList()
    {
        SetupDeliveredOrders();

        Assert.Empty(await _sut.GetTopProductsAsync());
    }

    // ?? Seeded tests — realistic data from samples/testdata.json ?????????????

    [Fact]
    public async Task GetVolumes_WithTestData_ReturnsTenUniqueProducts()
    {
        SeedFromTestData();

        Assert.Equal(10, (await _sut.GetVolumesAsync()).Count);
    }

    [Fact]
    public async Task GetVolumes_WithTestData_RankedByVolumeDescending()
    {
        SeedFromTestData();

        var result = await _sut.GetVolumesAsync();

        Assert.Equal(10,     result.Count);
        Assert.Equal("P007", result[0].ProduktId);
        Assert.Equal("P004", result[1].ProduktId);
        Assert.Equal("P001", result[2].ProduktId);
        Assert.Equal("P003", result[3].ProduktId);
        Assert.Equal("P010", result[4].ProduktId);
        Assert.Equal("P002", result[5].ProduktId);
        Assert.Equal("P006", result[6].ProduktId);
        Assert.Equal("P005", result[7].ProduktId);
        Assert.Equal("P008", result[8].ProduktId);
        Assert.Equal("P009", result[9].ProduktId);
    }

    [Fact]
    public async Task GetTopProducts_WithTestData_ReturnsExactlyTen()
    {
        SeedFromTestData();

        Assert.Equal(10, (await _sut.GetTopProductsAsync()).Count);
    }
}
