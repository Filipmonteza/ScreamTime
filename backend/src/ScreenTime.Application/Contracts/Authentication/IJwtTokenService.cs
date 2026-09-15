namespace ScreenTime.Application.Contracts.Authentication;

/// <summary>
/// Service responsible for generating JWT tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the given user, with one role claim per role.
    /// </summary>
    /// <param name="user">The authenticated user's token data.</param>
    /// <param name="roles">Every role the user holds. May be empty.</param>
    /// <returns>Token string and expiration timestamp.</returns>
    Task<(string Token, DateTime Expiration)> GenerateTokenAsync(UserTokenInfo user, IEnumerable<string> roles);
}
