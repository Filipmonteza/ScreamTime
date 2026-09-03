using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using System.Linq;
using ScreenTime.Application.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScreenTime.Infrastructure.Data;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

public class RegisterTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RegisterTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsCreatedAndToken()
    {
        // Arrange: create factory that replaces DB with InMemory
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ScreenTimeDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory DB
                services.AddDbContext<ScreenTimeDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        var client = factory.CreateClient();

        var request = new
        {
            email = $"test{Guid.NewGuid()}@example.com",
            password = "Test1234!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrEmpty();
        content.Role.Should().Be("Customer");
    }
}
