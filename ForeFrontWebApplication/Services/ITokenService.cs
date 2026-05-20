using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

/// <summary>Generates signed JWT tokens from a validated user identity.</summary>
public interface ITokenService
{
    /// <summary>
    /// Creates and signs a JWT for the given <paramref name="email"/> and <paramref name="role"/>.
    /// </summary>
    TokenResponse GenerateToken(string email, string role);
}
