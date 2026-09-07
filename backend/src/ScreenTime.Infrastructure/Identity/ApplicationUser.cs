using Microsoft.AspNetCore.Identity;

namespace ScreenTime.Infrastructure.Identity;

/// <summary>
/// Represents an authenticated user in the ScreenTime application.
/// Extends IdentityUser with Guid as the primary key type.
/// Contains additional profile fields used by the application.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // Optional user profile fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Nullable if the application doesn't require birthdate for all users
    public DateTime? Birthdate { get; set; }

    // Tracks when the user was created. Default to UTC now for new instances.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
