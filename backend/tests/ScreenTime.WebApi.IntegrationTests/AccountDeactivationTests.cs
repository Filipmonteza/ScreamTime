using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScreenTime.Infrastructure.Identity;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class AccountDeactivationTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public AccountDeactivationTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeactivateAccount_MarksUserAsDeactivated()
    {
        // Register a fresh customer and get an authorized client
        var client = await _factory.RegisterCustomerClientAsync();

        // Deactivate using the authenticated client
        var response = await client.PostAsync("/api/account/deactivate", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Extract user id from the token carried by the client
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        var userId = claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

        // Inspect persisted user state through the real services
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        user.Should().NotBeNull();
        user!.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
    }

    [Fact]
    public async Task DeactivatedUser_CannotAccessProtectedEndpoints_WithExistingToken()
    {
        // Register a fresh customer and get an authorized client
        var client = await _factory.RegisterCustomerClientAsync();

        // Deactivate the account
        var deactivate = await client.PostAsync("/api/account/deactivate", content: null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The same client (bearing the pre-deactivation JWT) should now be rejected
        var protectedResponse = await client.GetAsync("/api/bookings/current");
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_CanReactivate_DeactivatedUser()
    {
        // Register a fresh customer and get an authorized client
        var customer = await _factory.RegisterCustomerClientAsync();

        // Deactivate the account as the customer
        var deactivate = await customer.PostAsync("/api/account/deactivate", content: null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Extract user id from the token carried by the customer client
        var token = customer.DefaultRequestHeaders.Authorization!.Parameter!;
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        var userId = claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

        // Admin reactivates the user
        var admin = await _factory.CreateAdminClientAsync();
        var reactivate = await admin.PostAsync($"/api/admin/users/{userId}/reactivate", content: null);
        reactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Persisted state should show lockout cleared
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        user.Should().NotBeNull();
        user!.LockoutEnd.Should().BeNull();

        // The original token should now be accepted for protected endpoints again
        var protectedResponseAfter = await customer.GetAsync("/api/bookings/current");
        protectedResponseAfter.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
