namespace ScreenTime.Application.Contracts;

/// <summary>
/// Response DTO containing JWT token and user information.
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// JWT token to be sent in Authorization header.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (UTC).
    /// </summary>
    public DateTime Expiration { get; init; }

    /// <summary>
    /// Authenticated user's ID.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Authenticated user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's primary role (if assigned).
    /// </summary>
    public string? Role { get; init; }
}
