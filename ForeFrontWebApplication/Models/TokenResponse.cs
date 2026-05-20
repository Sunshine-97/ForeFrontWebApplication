namespace ForeFrontWebApplication.Models;

/// <summary>Successful authentication response containing the issued JWT.</summary>
public sealed class TokenResponse
{
    public required string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
}
