using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ForeFrontWebApplication.Models;
using ForeFrontWebApplication.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ForeFrontWebApplication.Services;

/// <summary>
/// Creates signed JWT tokens containing the caller's identity and role claim.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public TokenResponse GenerateToken(string email, string role)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var token = BuildToken(email, role, expiresAt);

        return new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    private JwtSecurityToken BuildToken(string email, string role, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role),
        ];

        return new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);
    }
}
