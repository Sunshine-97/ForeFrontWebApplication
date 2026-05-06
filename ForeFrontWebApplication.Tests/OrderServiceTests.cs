using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Xunit;

namespace ForeFrontWebApplication.Tests;

public class OrderServiceTests
{
    private readonly OrderService _sut = new();

    private static Order CreateValidOrder() => new()
    {
        Kund = new Customer { Namn = "Test Testsson", Email = "test@example.com" },
        Produkter = [new Product { Namn = "Widget", Antal = 2, Pris = 99.50m }]
    };

    [Fact]
    public void Create_ReturnsOrderWithGeneratedIdAndPendingStatus()
    {
        var order = CreateValidOrder();

        var result = _sut.Create(order);

        Assert.NotNull(result.OrderId);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public void GetAll_ReturnsAllCreatedOrders()
    {
        _sut.Create(CreateValidOrder());
        _sut.Create(CreateValidOrder());

        var result = _sut.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetById_ExistingOrder_ReturnsOrder()
    {
        var created = _sut.Create(CreateValidOrder());

        var result = _sut.GetById(created.OrderId);

        Assert.NotNull(result);
        Assert.Equal(created.OrderId, result.OrderId);
    }

    [Fact]
    public void GetById_NonExistingOrder_ReturnsNull()
    {
        var result = _sut.GetById("non-existing-id");

        Assert.Null(result);
    }

    [Fact]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        var created = _sut.Create(CreateValidOrder());

        var result = _sut.UpdateStatus(created.OrderId, OrderStatus.Confirmed);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        var created = _sut.Create(CreateValidOrder());

        Assert.Throws<InvalidOperationException>(
            () => _sut.UpdateStatus(created.OrderId, OrderStatus.Shipped));
    }

    [Fact]
    public void UpdateStatus_NonExistingOrder_ReturnsNull()
    {
        var result = _sut.UpdateStatus("non-existing-id", OrderStatus.Confirmed);

        Assert.Null(result);
    }

    [Fact]
    public void UpdateStatus_FullFlow_Succeeds()
    {
        var created = _sut.Create(CreateValidOrder());

        _sut.UpdateStatus(created.OrderId, OrderStatus.Confirmed);
        _sut.UpdateStatus(created.OrderId, OrderStatus.Shipped);
        var result = _sut.UpdateStatus(created.OrderId, OrderStatus.Delivered);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Delivered, result.Status);
    }

    [Fact]
    public void Delete_ExistingOrder_ReturnsTrue()
    {
        var created = _sut.Create(CreateValidOrder());

        var result = _sut.Delete(created.OrderId);

        Assert.True(result);
        Assert.Null(_sut.GetById(created.OrderId));
    }

    [Fact]
    public void Delete_NonExistingOrder_ReturnsFalse()
    {
        var result = _sut.Delete("non-existing-id");

        Assert.False(result);
    }
}
