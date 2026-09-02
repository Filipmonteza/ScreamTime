using Microsoft.AspNetCore.Identity;
using ScreenTime.Domain.Entities;

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
            return; // User already exists
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
}
