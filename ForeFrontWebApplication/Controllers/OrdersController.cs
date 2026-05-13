using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize(Policy = "ReadOnly")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    [Authorize(Policy = "Warehouse")]
    [ProducesResponseType(typeof(IReadOnlyList<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAll()
    {
        return Ok(_orderService.GetAll());
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Warehouse")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult GetById(string id)
    {
        var order = _orderService.GetById(id);
        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = new Order
        {
            Kund = request.Kund,
            Produkter = request.Produkter
        };

        var created = _orderService.Create(order);

        _logger.LogInformation("Order {OrderId} created by {UserId}",
            created.OrderId, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "Admin")]
    [Authorize(Policy = "Warehouse")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updated = _orderService.UpdateStatus(id, request.Status);
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
    [Authorize(Policy = "Admin")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Delete(string id)
    {
        if (!_orderService.Delete(id))
            return NotFound();

        _logger.LogWarning("Order {OrderId} deleted by {UserId}", id, User.Identity?.Name);

        return NoContent();
    }

    [HttpGet("volumes")]
    [Authorize(Policy = "Warehouse")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetVolumes([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if(from > to)
            return BadRequest(new { error = "Parametern 'from' måste vara tidigare än 'to'." });
        var volumes = _orderService.GetVolumes(from, to);
        return Ok(volumes);
    }
}

