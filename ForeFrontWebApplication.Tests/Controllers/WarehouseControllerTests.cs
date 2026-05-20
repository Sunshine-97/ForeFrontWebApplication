using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Security.Claims;
using System.Threading;
using Xunit;

namespace ForeFrontWebApplication.Tests.Controllers;

public class WarehouseControllerTests
{
    private readonly IWarehouseService _service = Substitute.For<IWarehouseService>();
    private readonly WarehouseController _sut;

    public WarehouseControllerTests()
    {
        _sut = new WarehouseController(_service, NullLogger<WarehouseController>.Instance)
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

    private static IReadOnlyList<OrderVolumes> FakeVolumes() =>
    [
        new OrderVolumes { ProduktId = "P001", Namn = "Skruv M6", Antal = 700 },
        new OrderVolumes { ProduktId = "P002", Namn = "Bult M8",  Antal = 165 },
    ];

    // ?? GetVolumes ????????????????????????????????????????????????????????????

    [Fact]
    public async Task GetVolumes_NoDates_ReturnsOkWithVolumes()
    {
        _service.GetVolumesAsync().Returns(FakeVolumes());

        var result = await _sut.GetVolumes(null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(2, Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value).Count);
    }

    [Fact]
    public async Task GetVolumes_WithDateRange_PassesDatesToService()
    {
        var from = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        _service.GetVolumesAsync(from, to).Returns(FakeVolumes());

        var result = await _sut.GetVolumes(from, to, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        await _service.Received(1).GetVolumesAsync(from, to);
    }

    [Fact]
    public async Task GetVolumes_FromAfterTo_ReturnsBadRequest()
    {
        var result = await _sut.GetVolumes(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await _service.DidNotReceive().GetVolumesAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>());
    }

    [Fact]
    public async Task GetVolumes_EmptyResult_ReturnsOkWithEmptyList()
    {
        _service.GetVolumesAsync().Returns(Array.Empty<OrderVolumes>());

        var result = await _sut.GetVolumes(null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value));
    }

    // ?? GetTopProducts ????????????????????????????????????????????????????????

    [Fact]
    public async Task GetTopProducts_ReturnsOkWithList()
    {
        _service.GetTopProductsAsync().Returns(FakeVolumes());

        var result = await _sut.GetTopProducts(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value);
    }

    [Fact]
    public async Task GetTopProducts_EmptyResult_ReturnsOkWithEmptyList()
    {
        _service.GetTopProductsAsync().Returns(Array.Empty<OrderVolumes>());

        var result = await _sut.GetTopProducts(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value));
    }

    [Fact]
    public async Task GetTopProducts_ServiceCalledOnce()
    {
        _service.GetTopProductsAsync().Returns(Array.Empty<OrderVolumes>());

        await _sut.GetTopProducts(CancellationToken.None);

        await _service.Received(1).GetTopProductsAsync();
    }
}
