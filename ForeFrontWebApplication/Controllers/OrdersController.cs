using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Queries;
using ForeFrontWebApplication.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(IReadOnlyList<Orders>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetAllOrdersQuery(), ct));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(Orders), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), ct);
        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create([FromBody] OrderRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _mediator.Send(
            new CreateOrderCommand(request.KundId, request.Produkter), ct);

        _logger.LogInformation("Order {OrderId} created by {UserId}",
            created.OrderId, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(Orders), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _mediator.Send(
            new UpdateOrderStatusCommand(id, request.Status), ct);
        if (updated is null)
            return NotFound();

        _logger.LogInformation("Order {OrderId} status updated to {Status} by {UserId}",
            id, request.Status, User.Identity?.Name);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!await _mediator.Send(new DeleteOrderCommand(id), ct))
            return NotFound();

        _logger.LogWarning("Order {OrderId} deleted by {UserId}", id, User.Identity?.Name);

        return NoContent();
    }
}
