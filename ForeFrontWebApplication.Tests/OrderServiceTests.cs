using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Xunit;

namespace ForeFrontWebApplication.Tests;

public class OrderServiceTests
{
    private readonly OrderService _sut = new();

    private static Customer FakeCustomer(string name, string email) => new()
    {
        Namn = name,
        Email = email
    };

    private static Order FakeOrder(string id, Customer customer, IList<Product> products, OrderStatus status, DateTime Created) => new()
    {
        OrderId = id,
        Kund = customer,
        Produkter = products,
        Status = status,
        Created = Created
    };

    private static IList<Product> FakeProduct(string id, string name, int quantity, decimal price) => [new Product
    {
        ProduktId = id,
        Namn = name,
        Antal = quantity,
        Pris = price
    }];

    private static Order BuildOrder() => new()
    {
        Kund = new Customer { Namn = "Test Testsson", Email = "test@example.com" },
        Produkter = [new Product { ProduktId = "P001", Namn = "Widget", Antal = 2, Pris = 99.50m }]
    };

    [Fact]
    public void Create_AssignsGeneratedIdAndSetsInitialState()
    {
        var result = _sut.Create(BuildOrder());

        Assert.NotNull(result.OrderId);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.True(result.Created <= DateTime.UtcNow);
    }

    [Fact]
    public void GetAll_ReturnsAllCreatedOrders()
    {
        _sut.Create(BuildOrder());
        _sut.Create(BuildOrder());

        Assert.Equal(2, _sut.GetAll().Count);
    }

    [Fact]
    public void GetById_ExistingOrder_ReturnsOrder()
    {
        var created = _sut.Create(BuildOrder());

        var result = _sut.GetById(created.OrderId);

        Assert.NotNull(result);
        Assert.Equal(created.OrderId, result.OrderId);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(_sut.GetById("unknown"));
    }

    [Fact]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        var created = _sut.Create(BuildOrder());

        var result = _sut.UpdateStatus(created.OrderId, OrderStatus.Confirmed);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public void UpdateStatus_UnknownId_ReturnsNull()
    {
        Assert.Null(_sut.UpdateStatus("unknown", OrderStatus.Confirmed));
    }

    [Fact]
    public void UpdateStatus_FullFlow_ReachesDelivered()
    {
        var created = _sut.Create(BuildOrder());

        _sut.UpdateStatus(created.OrderId, OrderStatus.Confirmed);
        _sut.UpdateStatus(created.OrderId, OrderStatus.Shipped);
        var result = _sut.UpdateStatus(created.OrderId, OrderStatus.Delivered);

        Assert.Equal(OrderStatus.Delivered, result?.Status);
    }

    [Fact]
    public void Delete_ExistingOrder_RemovesAndReturnsTrue()
    {
        var created = _sut.Create(BuildOrder());

        var deleted = _sut.Delete(created.OrderId);

        Assert.True(deleted);
        Assert.Null(_sut.GetById(created.OrderId));
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        Assert.False(_sut.Delete("unknown"));
    }

    [Fact]
    public void Volumes_All()
    {
        // Arrange
        var today = DateTime.UtcNow;

        var order1 = FakeOrder("ORD-1",
            FakeCustomer("Test Testsson", "test1@example.com"),
            FakeProduct("p1", "Skruv M6", 200, 2),
            OrderStatus.Delivered, today);

        var order2 = FakeOrder("ORD-2",
            FakeCustomer("Anna Andersson", "anna@example.com"),
            FakeProduct("p2", "Bult M8", 50, 5),
            OrderStatus.Delivered, today);

        var order3 = FakeOrder("ORD-3",
            FakeCustomer("Erik Eriksson", "erik@example.com"),
            FakeProduct("p3", "Mutter M6", 100, 1),
            OrderStatus.Pending, today);

        var order4 = FakeOrder("ORD-4",
            FakeCustomer("Lisa Larsson", "Lisa@example.com"),
            FakeProduct("p1", "Skruv M6", 150, 2),
            OrderStatus.Delivered, today);

        _sut.Create(order1);
        _sut.Create(order2);
        _sut.Create(order3);
        _sut.Create(order4);

        // Act
        var result = _sut.GetVolumes();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal("Skruv M6", result[0].Namn);
        Assert.Equal(350, result[0].Antal);

        Assert.Equal("Bult M8", result[1].Namn);
        Assert.Equal(50, result[1].Antal);
    }

    [Fact]
    public void Volumes_ByPeriod()
    {
        // Arrange
        var today = DateTime.UtcNow;

        var order1 = FakeOrder("ORD-1",
            FakeCustomer("Test Testsson", "test1@example.com"),
            FakeProduct("p1", "Skruv M6", 200, 2),
            OrderStatus.Delivered, today);

        var order2 = FakeOrder("ORD-2",
            FakeCustomer("Anna Andersson", "anna@example.com"),
            FakeProduct("p2", "Bult M8", 50, 5),
            OrderStatus.Delivered, today);

        var order3 = FakeOrder("ORD-3",
            FakeCustomer("Erik Eriksson", "erik@example.com"),
            FakeProduct("p3", "Mutter M6", 100, 1),
            OrderStatus.Delivered, today.AddDays(-9));

        var order4 = FakeOrder("ORD-4",
            FakeCustomer("Lisa Larsson", "Lisa@example.com"),
            FakeProduct("p1", "Skruv M6", 150, 2),
            OrderStatus.Delivered, today.AddDays(-8));

        _sut.Create(order1);
        _sut.Create(order2);
        _sut.Create(order3);
        _sut.Create(order4);

        // Act
        var result = _sut.GetVolumes(today.AddDays(-14), today.AddDays(-7));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal("Skruv M6", result[0].Namn);
        Assert.Equal(150, result[0].Antal);

        Assert.Equal("Mutter M6", result[1].Namn);
        Assert.Equal(100, result[1].Antal);
    }

    [Fact]
    public void Volumes_ByPeriod_NoContent()
    {
        // Arrange
        var today = DateTime.UtcNow;

        var order1 = FakeOrder("ORD-1",
            FakeCustomer("Test Testsson", "test1@example.com"),
            FakeProduct("p1", "Skruv M6", 200, 2),
            OrderStatus.Delivered, today);

        var order2 = FakeOrder("ORD-2",
            FakeCustomer("Anna Andersson", "anna@example.com"),
            FakeProduct("p2", "Bult M8", 50, 5),
            OrderStatus.Delivered, today);

        _sut.Create(order1);
        _sut.Create(order2);


        // Act
        var result = _sut.GetVolumes(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.Empty(result);

    }

}
