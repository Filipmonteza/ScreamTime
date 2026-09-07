using Microsoft.AspNetCore.Identity;

namespace ScreenTime.Infrastructure.Identity;

/// <summary>
/// Represents an authenticated user in the ScreenTime application.
/// Extends IdentityUser with Guid as the primary key type.
/// Contains additional profile fields used by the application.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{


}
