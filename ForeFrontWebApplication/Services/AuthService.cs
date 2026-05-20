using System.Security.Cryptography;
using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Services;

/// <summary>
/// Validates credentials against an in-memory user store using PBKDF2-SHA256 password hashing.
/// </summary>
/// <remarks>
/// <b>Development only:</b> replace with a database-backed user store before deploying to production.
/// Seed credentials are listed below for local testing:
/// <list type="table">
///   <listheader><term>Email</term><term>Password</term><term>Role</term></listheader>
///   <item><term>admin@forefront.dev</term><term>Admin@1234</term><term>Admin</term></item>
///   <item><term>warehouse@forefront.dev</term><term>Warehouse@1234</term><term>Warehouse</term></item>
///   <item><term>customer@forefront.dev</term><term>Customer@1234</term><term>Customer</term></item>
/// </list>
/// </remarks>
public sealed class AuthService : IAuthService
{
    private const int Iterations = 100_000;
    private const int HashByteLength = 32;

    // Salts and hashes are computed once at startup; plaintext passwords are not retained.
    private static readonly IReadOnlyList<UserRecord> Users = SeedUsers();

    private readonly ITokenService _tokenService;

    public AuthService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public TokenResponse? Authenticate(string email, string password)
    {
        var user = FindUser(email);
        if (user is null || !VerifyPassword(password, user.Salt, user.PasswordHash))
            return null;

        return _tokenService.GenerateToken(user.Email, user.Role);
    }

    private static UserRecord? FindUser(string email) =>
        Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

    private static bool VerifyPassword(string password, byte[] salt, string expectedHash)
    {
        var actualHashBytes = ComputeHash(password, salt);
        var expectedHashBytes = Convert.FromBase64String(expectedHash);

        // Constant-time comparison prevents timing-based user enumeration
        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password, salt, Iterations, HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(HashByteLength);
    }

    private static UserRecord CreateUser(string email, string password, string role)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Convert.ToBase64String(ComputeHash(password, salt));

        return new UserRecord
        {
            Email = email,
            Role = role,
            Salt = salt,
            PasswordHash = hash
        };
    }

    private static IReadOnlyList<UserRecord> SeedUsers() =>
    [
        CreateUser("admin@forefront.dev",     "Admin@1234",     "Admin"),
        CreateUser("warehouse@forefront.dev", "Warehouse@1234", "Warehouse"),
        CreateUser("customer@forefront.dev",  "Customer@1234",  "Customer"),
    ];
}
