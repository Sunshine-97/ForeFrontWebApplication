using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize(Roles = "Warehouse,Admin")]
public sealed class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _logger           = logger;
    }

    /// <summary>
    /// Returns aggregated product volumes across all delivered orders,
    /// optionally filtered to a date range.
    /// </summary>
    [HttpGet("volumes")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderVolumes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVolumes(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        if (from > to)
            return BadRequest(new { error = "Parametern 'from' måste vara tidigare än 'to'." });

        _logger.LogInformation("Volumes requested by {UserId} from={From} to={To}",
            User.Identity?.Name, from, to);

        return Ok(await _warehouseService.GetVolumesAsync(from, to, ct));
    }

    /// <summary>
    /// Returns the top 10 products by total sold quantity across all delivered orders.
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderVolumes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts(CancellationToken ct)
    {
        _logger.LogInformation("Top products requested by {UserId}", User.Identity?.Name);

        return Ok(await _warehouseService.GetTopProductsAsync(ct));
    }
}
