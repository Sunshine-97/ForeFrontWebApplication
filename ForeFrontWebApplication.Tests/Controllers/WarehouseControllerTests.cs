using ForeFrontWebApplication.Controllers;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Queries;
using MediatR;
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
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly WarehouseController _sut;

    public WarehouseControllerTests()
    {
        _sut = new WarehouseController(_mediator, NullLogger<WarehouseController>.Instance)
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
        _mediator.Send(Arg.Any<GetVolumesQuery>(), Arg.Any<CancellationToken>())
                 .Returns(FakeVolumes());

        var result = await _sut.GetVolumes(null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(2, Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value).Count);
    }

    [Fact]
    public async Task GetVolumes_WithDateRange_PassesDatesToService()
    {
        var from = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        _mediator.Send(Arg.Any<GetVolumesQuery>(), Arg.Any<CancellationToken>())
                 .Returns(FakeVolumes());

        var result = await _sut.GetVolumes(from, to, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        await _mediator.Received(1).Send(
            Arg.Is<GetVolumesQuery>(q => q.From == from && q.To == to),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVolumes_FromAfterTo_ReturnsBadRequest()
    {
        var result = await _sut.GetVolumes(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        await _mediator.DidNotReceive().Send(Arg.Any<GetVolumesQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVolumes_EmptyResult_ReturnsOkWithEmptyList()
    {
        _mediator.Send(Arg.Any<GetVolumesQuery>(), Arg.Any<CancellationToken>())
                 .Returns(Array.Empty<OrderVolumes>());

        var result = await _sut.GetVolumes(null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value));
    }

    // ?? GetTopProducts ????????????????????????????????????????????????????????

    [Fact]
    public async Task GetTopProducts_ReturnsOkWithList()
    {
        _mediator.Send(Arg.Any<GetTopProductsQuery>(), Arg.Any<CancellationToken>())
                 .Returns(FakeVolumes());

        var result = await _sut.GetTopProducts(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value);
    }

    [Fact]
    public async Task GetTopProducts_EmptyResult_ReturnsOkWithEmptyList()
    {
        _mediator.Send(Arg.Any<GetTopProductsQuery>(), Arg.Any<CancellationToken>())
                 .Returns(Array.Empty<OrderVolumes>());

        var result = await _sut.GetTopProducts(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<OrderVolumes>>(ok.Value));
    }

    [Fact]
    public async Task GetTopProducts_ServiceCalledOnce()
    {
        _mediator.Send(Arg.Any<GetTopProductsQuery>(), Arg.Any<CancellationToken>())
                 .Returns(Array.Empty<OrderVolumes>());

        await _sut.GetTopProducts(CancellationToken.None);

        await _mediator.Received(1).Send(Arg.Any<GetTopProductsQuery>(), Arg.Any<CancellationToken>());
    }
}
