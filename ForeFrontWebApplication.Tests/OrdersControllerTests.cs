using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using Xunit;

namespace ForeFrontWebApplication.Tests;

public class OrdersControllerTests
{
    private readonly IOrderService _service = Substitute.For<IOrderService>();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_service, NullLogger<OrdersController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };
    }

    private static Order BuildOrder(string id = "order-1") => new()
    {
        OrderId = id,
        Kund = new Customer { Namn = "Test Testsson", Email = "test@example.com" },
        Produkter = [new Product {ProduktId = "p1", Namn = "Widget", Antal = 1, Pris = 50m }],
        Status = OrderStatus.Pending
    };

    private static CreateOrderRequest BuildCreateRequest() => new()
    {
        Kund = new Customer { Namn = "Test Testsson", Email = "test@example.com" },
        Produkter = [new Product {ProduktId = "p1", Namn = "Widget", Antal = 1, Pris = 50m }]
    };

    [Fact]
    public void GetAll_ReturnsOkWithOrders()
    {
        _service.GetAll().Returns([BuildOrder()]);

        var result = _sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var orders = Assert.IsAssignableFrom<IReadOnlyList<Order>>(ok.Value);
        Assert.Single(orders);
    }

    [Fact]
    public void GetById_ExistingOrder_ReturnsOk()
    {
        var order = BuildOrder();
        _service.GetById(order.OrderId).Returns(order);

        var result = _sut.GetById(order.OrderId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order.OrderId, Assert.IsType<Order>(ok.Value).OrderId);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNotFound()
    {
        _service.GetById(Arg.Any<string>()).Returns((Order?)null);

        Assert.IsType<NotFoundResult>(_sut.GetById("unknown"));
    }

    [Fact]
    public void Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var order = BuildOrder();
        _service.Create(Arg.Any<Order>()).Returns(order);

        var result = _sut.Create(BuildCreateRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(OrdersController.GetById), created.ActionName);
        Assert.Equal(order.OrderId, created.RouteValues?["id"]);
        Assert.Equal(OrderStatus.Pending, Assert.IsType<Order>(created.Value).Status);
    }

    [Fact]
    public void Create_InvalidModelState_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Kund", "Required");

        Assert.IsType<BadRequestObjectResult>(_sut.Create(BuildCreateRequest()));
    }

    [Fact]
    public void UpdateStatus_ValidTransition_ReturnsOk()
    {
        var order = BuildOrder();
        order.Status = OrderStatus.Confirmed;
        _service.UpdateStatus(order.OrderId, OrderStatus.Confirmed).Returns(order);

        var result = _sut.UpdateStatus(order.OrderId, new UpdateStatusRequest { Status = OrderStatus.Confirmed });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(OrderStatus.Confirmed, Assert.IsType<Order>(ok.Value).Status);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ReturnsBadRequest()
    {
        _service.UpdateStatus(Arg.Any<string>(), Arg.Any<OrderStatus>())
            .Throws(new InvalidOperationException());

        Assert.IsType<BadRequestObjectResult>(
            _sut.UpdateStatus("order-1", new UpdateStatusRequest { Status = OrderStatus.Delivered }));
    }

    [Fact]
    public void UpdateStatus_UnknownId_ReturnsNotFound()
    {
        _service.UpdateStatus(Arg.Any<string>(), Arg.Any<OrderStatus>()).Returns((Order?)null);

        Assert.IsType<NotFoundResult>(
            _sut.UpdateStatus("unknown", new UpdateStatusRequest { Status = OrderStatus.Confirmed }));
    }

    [Fact]
    public void Delete_ExistingOrder_ReturnsNoContent()
    {
        _service.Delete("order-1").Returns(true);

        Assert.IsType<NoContentResult>(_sut.Delete("order-1"));
    }

    [Fact]
    public void Delete_UnknownId_ReturnsNotFound()
    {
        _service.Delete(Arg.Any<string>()).Returns(false);

        Assert.IsType<NotFoundResult>(_sut.Delete("unknown"));
    }

    [Fact]
    public void Volumes_ByPeriod_Invalid_Date()
    {
        // Arrange
        _service.GetVolumes(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Throws(new ArgumentException("Invalid date range"));

        // Act
        var result = _sut.GetVolumes(DateTime.UtcNow, DateTime.UtcNow.AddDays(-7));

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

}
