namespace ForeFrontWebApplication.Settings;

/// <summary>
/// Typed configuration for JWT token generation and validation.
/// <para>
/// <b>Production note:</b> supply <see cref="SigningKey"/> via an environment variable or
/// a secrets manager (e.g. Azure Key Vault). Never commit a real key to source control.
/// </para>
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Symmetric signing key. Must be at least 32 characters long.</summary>
    public required string SigningKey { get; init; }

    /// <summary>Token issuer — must match the value validated by consumers.</summary>
    public required string Issuer { get; init; }

    /// <summary>Intended audience — must match the value validated by consumers.</summary>
    public required string Audience { get; init; }

    /// <summary>Token lifetime in minutes. Defaults to 60.</summary>
    public int ExpiryMinutes { get; init; } = 60;
}
