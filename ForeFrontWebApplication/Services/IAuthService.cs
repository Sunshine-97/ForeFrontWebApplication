using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

/// <summary>Validates caller credentials and, on success, returns a signed token.</summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates <paramref name="email"/> and <paramref name="password"/> against the user store.
    /// </summary>
    /// <returns>
    /// A <see cref="TokenResponse"/> on success, or <c>null</c> when credentials are invalid.
    /// </returns>
    TokenResponse? Authenticate(string email, string password);
}
