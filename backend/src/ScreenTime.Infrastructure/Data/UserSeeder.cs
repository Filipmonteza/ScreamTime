using Microsoft.AspNetCore.Identity;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Seeds a test user for development/testing purposes.
/// </summary>
public static class UserSeeder
{
    public static async Task SeedTestUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string testEmail = "test@screentime.com";
        const string testPassword = "Test1234!";

        var existingUser = await userManager.FindByEmailAsync(testEmail);
        if (existingUser != null)
        {
            // Existing databases predate the role assignment below.
            await EnsureCustomerRoleAsync(userManager, existingUser);
            return;
        }

        var testUser = new ApplicationUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(testUser, testPassword);

        if (result.Succeeded)
        {
            await EnsureCustomerRoleAsync(userManager, testUser);

            Console.WriteLine($"✅ Test user created successfully:");
            Console.WriteLine($"   Email: {testEmail}");
            Console.WriteLine($"   Password: {testPassword}");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create test user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Description}");
            }
        }
    }

    private static async Task EnsureCustomerRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, IdentitySeeder.CustomerRole))
        {
            await userManager.AddToRoleAsync(user, IdentitySeeder.CustomerRole);
        }
    }
}
