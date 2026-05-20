using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ForeFrontWebApplication.Controllers;

[ApiController]
[Route("auth")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Issues a JWT for valid credentials. The token carries the caller's role claim
    /// and must be sent as a Bearer token on subsequent requests.
    /// </summary>
    [HttpPost("token")]
    [EnableRateLimiting("Mutate")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Token([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = _authService.Authenticate(request.Email, request.Password);

        if (response is null)
        {
            _logger.LogWarning("Failed authentication attempt");
            return Unauthorized();
        }

        _logger.LogInformation("Token issued successfully");
        return Ok(response);
    }
}
