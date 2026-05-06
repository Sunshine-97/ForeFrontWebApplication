using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Hämta alla ordrar.</summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_orderService.GetAll());
    }

    /// <summary>Hämta en specifik order via ID.</summary>
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var order = _orderService.GetById(id);
        if (order is null)
            return NotFound();

        return Ok(order);
    }

    /// <summary>Skapa en ny order.</summary>
    [HttpPost]
    public IActionResult Create([FromBody] Order order)
    {
        var created = _orderService.Create(order);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    /// <summary>Uppdatera orderstatus.</summary>
    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updated = _orderService.UpdateStatus(id, request.Status);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Ta bort en order.</summary>
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!_orderService.Delete(id))
            return NotFound();

        return NoContent();
    }
}
