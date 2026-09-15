using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class RoleAuthorizationTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public RoleAuthorizationTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Customer_CannotAccess_AdminEndpoints()
    {
        var client = _factory.CreateClient();

        var email = $"itest{Guid.NewGuid():N}@example.com";
        var password = "Test1234!";

        // Register as a Customer
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

        // Login
        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new { email, password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginContent.Should().NotBeNull();
        var token = loginContent!.Token;
        token.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Attempt to call admin endpoint
        var adminResponse = await client.GetAsync("/api/Admin/users");

        // Customer must not be allowed
        adminResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
