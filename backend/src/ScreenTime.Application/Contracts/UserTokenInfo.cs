namespace ScreenTime.Application.Contracts;

/// <summary>
/// The user data required to mint a JWT. Exists so the Application layer can
/// describe token generation without depending on ASP.NET Identity's
/// ApplicationUser, which lives in Infrastructure.
/// </summary>
public record UserTokenInfo
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;
}
