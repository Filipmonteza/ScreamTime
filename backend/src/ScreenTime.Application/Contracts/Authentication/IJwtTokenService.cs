namespace ScreenTime.Application.Contracts.Authentication;

/// <summary>
/// Service responsible for generating JWT tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the given user and role.
    /// </summary>
    /// <param name="user">The authenticated user's token data.</param>
    /// <param name="role">The user's primary role (if assigned).</param>
    /// <returns>Token string and expiration timestamp.</returns>
    Task<(string Token, DateTime Expiration)> GenerateTokenAsync(UserTokenInfo user, string? role = null);
}
