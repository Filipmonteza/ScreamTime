using Microsoft.AspNetCore.Identity;
using ScreenTime.Infrastructure.Identity;
namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Seeds test users for development/testing purposes.
/// </summary>
public static class UserSeeder
{
    public static async Task SeedTestUserAsync(UserManager<ApplicationUser> userManager)
    {
        await SeedUserAsync(userManager, "test@screentime.com", "Test1234!", IdentitySeeder.CustomerRole);
        await SeedUserAsync(userManager, "admin@screentime.com", "Admin1234!", IdentitySeeder.AdminRole);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Existing databases predate the role assignment below.
            await EnsureRoleAsync(userManager, existingUser, role);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await EnsureRoleAsync(userManager, user, role);
            Console.WriteLine($"✅ Test user created successfully:");
            Console.WriteLine($"   Email: {email}");
            Console.WriteLine($"   Password: {password}");
            Console.WriteLine($"   Role: {role}");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create test user ({email}):");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Description}");
            }
        }
    }

    private static async Task EnsureRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string role)
    {
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}