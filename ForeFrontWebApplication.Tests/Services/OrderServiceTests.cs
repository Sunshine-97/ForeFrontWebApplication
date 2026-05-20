using FakeItEasy;
using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories.Customer;
using ForeFrontWebApplication.Repositories.Product;
using ForeFrontWebApplication.Repositories.Order;
using ForeFrontWebApplication.Services;
using NSubstitute;
using Xunit;

namespace ForeFrontWebApplication.Tests.Services;

public class OrderServiceTests
{
    private readonly IOrderRepository    _repository         = Substitute.For<IOrderRepository>();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository  _productRepository  = Substitute.For<IProductRepository>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_repository, _customerRepository, _productRepository);
    }

    // ?? Mocked data builders ??????????????????????????????????????????????????

    private static OrderRequest BuildRequest(string kundId = "K-001") => new()
    {
        KundId    = kundId,
        Produkter =
        [
            new OrderItemRequest { ProduktId = "P001", Antal = 2 },
            new OrderItemRequest { ProduktId = "P002", Antal = 5 },
        ]
    };

    private static OrderEntity BuildStoredOrder(string orderId = "order-1", OrderStatus status = OrderStatus.Pending) => new()
    {
        OrderId   = orderId,
        KundId    = "K-001",
        Status    = status,
        Created   = DateTime.UtcNow,
        Produkter =
        [
            new OrderLine { OrderLineId = "line-1", OrderId = orderId, ProductId = "P001", Namn = "Widget", Antal = 2, Pris = 10m }
        ]
    };

    // ?? Create ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task Create_AssignsGeneratedIdAndSetsInitialStatus()
    {
        _customerRepository.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(
            new Models.Product.Products { ProductId = "P001", Namn = "Widget", Pris = 10m });

        var result = await _sut.CreateAsync(BuildRequest());

        Assert.NotNull(result.OrderId);
        Assert.NotEmpty(result.OrderId);
        Assert.Equal(OrderStatus.Pending, result.Status);
        await _repository.Received(1).AddAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_CallsRepositoryAdd()
    {
        _customerRepository.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(
            new Models.Product.Products { ProductId = "P001", Namn = "Widget", Pris = 10m });

        await _sut.CreateAsync(BuildRequest());

        await _repository.Received(1).AddAsync(Arg.Is<OrderEntity>(o =>
            o.KundId == "K-001" &&
            o.Status == OrderStatus.Pending &&
            o.Produkter.Count == 2));
    }

    // ?? GetAll ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task GetAll_DelegatesToRepository()
    {
        var orders = new List<OrderEntity> { BuildStoredOrder("o1"), BuildStoredOrder("o2") }.AsReadOnly();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    // ?? GetById ???????????????????????????????????????????????????????????????

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOrder()
    {
        var order = BuildStoredOrder("order-1");
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.GetByIdAsync("order-1");

        Assert.NotNull(result);
        Assert.Equal("order-1", result.OrderId);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((OrderEntity?)null);

        Assert.Null(await _sut.GetByIdAsync("unknown"));
    }

    // ?? UpdateStatus

    [Fact]
    public async Task UpdateStatus_ValidTransition_UpdatesStatus()
    {
        var order = BuildStoredOrder("order-1", OrderStatus.Pending);
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateStatusAsync("order-1", OrderStatus.Confirmed);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task UpdateStatus_UnknownId_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((OrderEntity?)null);

        Assert.Null(await _sut.UpdateStatusAsync("unknown", OrderStatus.Confirmed));
    }

    [Fact]
    public async Task UpdateStatus_FullFlow_ReachesDelivered()
    {
        var order = BuildStoredOrder("order-1", OrderStatus.Shipped);
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateStatusAsync("order-1", OrderStatus.Delivered);

        Assert.Equal(OrderStatus.Delivered, result?.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending,   OrderStatus.Pending)]
    [InlineData(OrderStatus.Pending,   OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending,   OrderStatus.Delivered)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped,   OrderStatus.Pending)]
    [InlineData(OrderStatus.Shipped,   OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped,   OrderStatus.Shipped)]
    public async Task UpdateStatus_InvalidTransition_Throws(OrderStatus from, OrderStatus to)
    {
        var order = BuildStoredOrder("order-1", from);
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateStatusAsync("order-1", to));
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task UpdateStatus_FromTerminalState_Throws(OrderStatus terminal)
    {
        var order = BuildStoredOrder("order-1", terminal);
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateStatusAsync("order-1", OrderStatus.Pending));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped)]
    public async Task UpdateStatus_CancelFromActiveState_Succeeds(OrderStatus from)
    {
        var order = BuildStoredOrder("order-1", from);
        _repository.GetByIdAsync("order-1", Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateStatusAsync("order-1", OrderStatus.Cancelled);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Cancelled, result.Status);
    }

    // ?? Delete ????????????????????????????????????????????????????????????????

    [Fact]
    public async Task Delete_ExistingOrder_ReturnsTrue()
    {
        _repository.DeleteAsync("order-1", Arg.Any<CancellationToken>()).Returns(true);

        Assert.True(await _sut.DeleteAsync("order-1"));
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsFalse()
    {
        _repository.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        Assert.False(await _sut.DeleteAsync("unknown"));
    }
}
