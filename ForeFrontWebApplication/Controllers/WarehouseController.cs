using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize(Roles = "Warehouse,Admin")]
public sealed class WarehouseController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IMediator mediator, ILogger<WarehouseController> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    [HttpGet("volumes")]
    [EnableRateLimiting("ReadById")]
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

        return Ok(await _mediator.Send(new GetVolumesQuery(from, to), ct));
    }

    [HttpGet("top-products")]
    [EnableRateLimiting("ReadById")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderVolumes>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts(CancellationToken ct)
    {
        _logger.LogInformation("Top products requested by {UserId}", User.Identity?.Name);

        return Ok(await _mediator.Send(new GetTopProductsQuery(), ct));
    }
}
