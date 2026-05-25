using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Services;
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
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger       = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _orderService.GetAllAsync(ct));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
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

        var created = await _orderService.CreateAsync(request, ct);

        _logger.LogInformation("Order {OrderId} created by {UserId}",
            created.OrderId, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Warehouse")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
            return BadRequest(new { error = "Invalid status value." });

        try
        {
            var updated = await _orderService.UpdateStatusAsync(id, request.Status, ct);
            if (updated is null)
                return NotFound();

            _logger.LogInformation("Order {OrderId} status updated to {Status} by {UserId}",
                id, request.Status, User.Identity?.Name);

            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new { error = "Ogiltig statusövergång." });
        }
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
        if (!await _orderService.DeleteAsync(id, ct))
            return NotFound();

        _logger.LogWarning("Order {OrderId} deleted by {UserId}", id, User.Identity?.Name);

        return NoContent();
    }
}
