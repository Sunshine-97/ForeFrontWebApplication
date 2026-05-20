namespace ForeFrontWebApplication.Models;

/// <summary>
/// Internal representation of a user in the in-memory store.
/// Not exposed through any API endpoint.
/// </summary>
internal sealed class UserRecord
{
    public required string Email { get; init; }
    public required string Role { get; init; }

    /// <summary>Base64-encoded PBKDF2-SHA256 hash of the user's password.</summary>
    public required string PasswordHash { get; init; }

    /// <summary>Per-user random salt used when hashing the password.</summary>
    public required byte[] Salt { get; init; }
}
