using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ForeFrontWebApplication.Tests;

public class OrdersControllerTests
{
    private readonly OrderService _service = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_service);
    }

    private static Order CreateValidOrder() => new()
    {
        Kund = new Customer { Namn = "Test Testsson", Email = "test@example.com" },
        Produkter = [new Product { Namn = "Widget", Antal = 1, Pris = 50m }]
    };

    [Fact]
    public void GetAll_ReturnsOkWithOrders()
    {
        _service.Create(CreateValidOrder());

        var result = _sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var orders = Assert.IsAssignableFrom<IReadOnlyList<Order>>(ok.Value);
        Assert.Single(orders);
    }

    [Fact]
    public void GetById_ExistingOrder_ReturnsOk()
    {
        var created = _service.Create(CreateValidOrder());

        var result = _sut.GetById(created.OrderId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var order = Assert.IsType<Order>(ok.Value);
        Assert.Equal(created.OrderId, order.OrderId);
    }

    [Fact]
    public void GetById_NonExisting_ReturnsNotFound()
    {
        var result = _sut.GetById("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_ReturnsCreatedAtAction()
    {
        var result = _sut.Create(CreateValidOrder());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var order = Assert.IsType<Order>(created.Value);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void UpdateStatus_ValidTransition_ReturnsOk()
    {
        var order = _service.Create(CreateValidOrder());

        var result = _sut.UpdateStatus(order.OrderId, new UpdateStatusRequest { Status = OrderStatus.Confirmed });

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<Order>(ok.Value);
        Assert.Equal(OrderStatus.Confirmed, updated.Status);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ReturnsBadRequest()
    {
        var order = _service.Create(CreateValidOrder());

        var result = _sut.UpdateStatus(order.OrderId, new UpdateStatusRequest { Status = OrderStatus.Delivered });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void UpdateStatus_NonExisting_ReturnsNotFound()
    {
        var result = _sut.UpdateStatus("missing", new UpdateStatusRequest { Status = OrderStatus.Confirmed });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ExistingOrder_ReturnsNoContent()
    {
        var order = _service.Create(CreateValidOrder());

        var result = _sut.Delete(order.OrderId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_NonExisting_ReturnsNotFound()
    {
        var result = _sut.Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}
