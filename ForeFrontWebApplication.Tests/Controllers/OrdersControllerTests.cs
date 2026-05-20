using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using System.Threading;
using Xunit;

namespace ForeFrontWebApplication.Tests.Controllers;

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

    private static OrderEntity BuildOrder(string id = "order-1") => new()
    {
        OrderId   = id,
        KundId    = "customer-1",
        Produkter = [new OrderLine { OrderLineId = "line-1", OrderId = id, ProductId = "p1", Namn = "Widget", Antal = 1, Pris = 50m }],
        Status    = OrderStatus.Pending
    };

    private static OrderRequest BuildCreateRequest() => new()
    {
        KundId    = "customer-1",
        Produkter = [new OrderItemRequest { ProduktId = "p1", Antal = 1 }]
    };

    // ?? GetAll ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task GetAll_ReturnsOkWithOrders()
    {
        _service.GetAllAsync().Returns(new List<OrderEntity> { BuildOrder() }.AsReadOnly());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<OrderEntity>>(ok.Value));
    }

    // ?? GetById ???????????????????????????????????????????????????????????????

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOk()
    {
        var order = BuildOrder();
        _service.GetByIdAsync(order.OrderId).Returns(order);

        var result = await _sut.GetById(order.OrderId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order.OrderId, Assert.IsType<OrderEntity>(ok.Value).OrderId);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<string>()).Returns((OrderEntity?)null);

        Assert.IsType<NotFoundResult>(await _sut.GetById("unknown", CancellationToken.None));
    }

    // ?? Create ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var fakeResponse = new OrderResponse { OrderId = "order-1", Status = OrderStatus.Pending };
        _service.CreateAsync(Arg.Any<OrderRequest>()).Returns(fakeResponse);

        var result = await _sut.Create(BuildCreateRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(OrdersController.GetById), created.ActionName);
        Assert.Equal("order-1", created.RouteValues?["id"]);
        Assert.Equal(OrderStatus.Pending, Assert.IsType<OrderResponse>(created.Value).Status);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("KundId", "Required");

        Assert.IsType<BadRequestObjectResult>(
            await _sut.Create(BuildCreateRequest(), CancellationToken.None));
    }

    // ?? UpdateStatus ??????????????????????????????????????????????????????????

    [Fact]
    public async Task UpdateStatus_ValidTransition_ReturnsOk()
    {
        var order = BuildOrder();
        order.Status = OrderStatus.Confirmed;
        _service.UpdateStatusAsync(order.OrderId, OrderStatus.Confirmed).Returns(order);

        var result = await _sut.UpdateStatus(order.OrderId,
            new UpdateStatusRequest { Status = OrderStatus.Confirmed },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(OrderStatus.Confirmed, Assert.IsType<OrderEntity>(ok.Value).Status);
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_ReturnsBadRequest()
    {
        _service.UpdateStatusAsync(Arg.Any<string>(), Arg.Any<OrderStatus>())
                .Throws(new InvalidOperationException());

        Assert.IsType<BadRequestObjectResult>(
            await _sut.UpdateStatus("order-1",
                new UpdateStatusRequest { Status = OrderStatus.Delivered },
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatus_UnknownId_ReturnsNotFound()
    {
        _service.UpdateStatusAsync(Arg.Any<string>(), Arg.Any<OrderStatus>())
                .Returns((OrderEntity?)null);

        Assert.IsType<NotFoundResult>(
            await _sut.UpdateStatus("unknown",
                new UpdateStatusRequest { Status = OrderStatus.Confirmed },
                CancellationToken.None));
    }

    // ?? Delete ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task Delete_ExistingOrder_ReturnsNoContent()
    {
        _service.DeleteAsync("order-1").Returns(true);

        Assert.IsType<NoContentResult>(await _sut.Delete("order-1", CancellationToken.None));
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        _service.DeleteAsync(Arg.Any<string>()).Returns(false);

        Assert.IsType<NotFoundResult>(await _sut.Delete("unknown", CancellationToken.None));
    }
}
