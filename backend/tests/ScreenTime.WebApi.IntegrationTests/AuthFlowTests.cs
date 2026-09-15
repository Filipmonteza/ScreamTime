using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class AuthFlowTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public AuthFlowTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_AccessProtectedEndpoints_And_Logout()
    {
        var client = _factory.CreateClient();

        var email = $"itest{Guid.NewGuid():N}@example.com";
        var password = "Test1234!";

        // Register
        var registerRequest = new
        {
            email,
            password,
            confirmPassword = password
        };

        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        registerContent.Should().NotBeNull();
        registerContent!.Token.Should().NotBeNullOrEmpty();
        registerContent.Role.Should().Be("Customer");

        // Login (ensure login path also emits roles)
        var loginRequest = new { email, password };
        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginContent.Should().NotBeNull();
        var token = loginContent!.Token;
        token.Should().NotBeNullOrEmpty();

        // The role must be in the token itself, not only in the response body.
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Customer");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Call a protected endpoint (BookingsController is [Authorize])
        var bookingsResponse = await client.GetAsync("/api/Bookings/current");
        bookingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Logout
        var logoutResponse = await client.PostAsync("/api/Auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
