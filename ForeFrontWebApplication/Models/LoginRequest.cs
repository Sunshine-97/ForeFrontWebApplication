using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models;

/// <summary>Request body for the POST /auth/token endpoint.</summary>
public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public required string Email { get; init; }

    /// <summary>Caller-supplied password. Never logged or returned.</summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; init; }
}
