using Microsoft.AspNetCore.Identity;

namespace ScreenTime.Infrastructure.Identity;

/// <summary>
/// Seeds the fixed set of application roles. These are an invariant the API
/// relies on: registration assigns <see cref="CustomerRole"/> and assumes the
/// role already exists rather than creating it on the fly per request.
/// </summary>
public static class IdentitySeeder
{
    public const string AdminRole = "Admin";
    public const string StaffRole = "Staff";
    public const string CustomerRole = "Customer";

    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in new[] { AdminRole, StaffRole, CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
