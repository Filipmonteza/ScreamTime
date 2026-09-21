using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class StaffAccountFlowTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public StaffAccountFlowTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_CanCreate_Staff_And_Staff_CanLogin_With_StaffRole()
    {
        var client = _factory.CreateClient();

        // Login as seeded admin
        var adminLogin = new { email = "admin@screentime.com", password = "Admin1234!" };
        var adminLoginResponse = await client.PostAsJsonAsync("/api/Auth/login", adminLogin);
        adminLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        adminAuth.Should().NotBeNull();
        var adminToken = adminAuth!.Token;
        adminToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a new staff account
        var staffEmail = $"staff{Guid.NewGuid():N}@example.com";
        var staffPassword = "Test1234!";

        var createRequest = new { email = staffEmail, password = staffPassword, roles = new[] { "Staff" } };
        var createResponse = await client.PostAsJsonAsync("/api/Admin/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login with the newly created staff account
        var staffLoginResponse = await client.PostAsJsonAsync("/api/Auth/login", new { email = staffEmail, password = staffPassword });
        staffLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var staffAuth = await staffLoginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        staffAuth.Should().NotBeNull();
        staffAuth!.Role.Should().Be("Staff");
        var token = staffAuth.Token;
        token.Should().NotBeNullOrEmpty();

        // Token must contain Staff role claim
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Staff");
    }
}
