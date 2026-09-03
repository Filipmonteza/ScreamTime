using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class RegisterTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public RegisterTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsCreatedWithCustomerToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            email = $"test{Guid.NewGuid():N}@example.com",
            password = "Test1234!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrEmpty();
        content.Email.Should().Be(request.email);
        content.Role.Should().Be("Customer");
    }
}
