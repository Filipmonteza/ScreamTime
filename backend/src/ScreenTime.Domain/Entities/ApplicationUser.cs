using Microsoft.AspNetCore.Identity;

namespace ScreenTime.Domain.Entities;

/// <summary>
/// Represents an authenticated user in the ScreenTime application.
/// Extends IdentityUser with Guid as the primary key type.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
 
}
