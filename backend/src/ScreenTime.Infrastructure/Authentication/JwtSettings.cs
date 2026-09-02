namespace ScreenTime.Infrastructure.Authentication;

/// <summary>
/// Configuration options for JWT token generation.
/// Binds to the "Jwt" section in appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key used to sign JWT tokens (must be at least 32 characters for HS256).
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// Token issuer (typically the API URL or name).
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Token audience (typically the client application identifier).
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Token lifetime in minutes (default: 60 minutes).
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;
}
