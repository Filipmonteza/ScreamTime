using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Signed-in clients and the set-up calls the tests start from. Everything
/// goes through the real API rather than straight to the DbContext.
/// </summary>
internal static class TestApi
{
    // Both accounts come from UserSeeder, which runs in the test host too.
    public static Task<HttpClient> CreateAdminClientAsync(this ScreenTimeApiFactory factory) =>
        factory.SignInAsync("admin@screentime.com", "Admin1234!");

    public static Task<HttpClient> CreateCustomerClientAsync(this ScreenTimeApiFactory factory) =>
        factory.SignInAsync("test@screentime.com", "Test1234!");

    // A customer account of its own, for the tests that need two people.
    public static async Task<HttpClient> RegisterCustomerClientAsync(
        this ScreenTimeApiFactory factory)
    {
        var client = factory.CreateClient();
        const string password = "Test1234!";

        var response = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                email = $"kund{Guid.NewGuid():N}@example.com",
                password,
                confirmPassword = password
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return Authorize(client, await ReadAuthAsync(response));
    }

    public static async Task<AuditoriumDto> CreateAuditoriumAsync(
        this HttpClient client,
        string name)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auditoriums",
            new AuditoriumCreateDto(name));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<AuditoriumDto>())!;
    }

    public static async Task<List<AuditoriumListDto>> GetAuditoriumsAsync(
        this HttpClient client,
        string query = "") =>
        (await client.GetFromJsonAsync<List<AuditoriumListDto>>(
            $"/api/auditoriums{query}"))!;

    public static async Task<AuditoriumDetailsDto> GetAuditoriumDetailsAsync(
        this HttpClient client,
        Guid auditoriumId) =>
        (await client.GetFromJsonAsync<AuditoriumDetailsDto>(
            $"/api/auditoriums/{auditoriumId}"))!;

    private static async Task<HttpClient> SignInAsync(
        this ScreenTimeApiFactory factory,
        string email,
        string password)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return Authorize(client, await ReadAuthAsync(response));
    }

    private static async Task<AuthResponse> ReadAuthAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<AuthResponse>())!;

    private static HttpClient Authorize(HttpClient client, AuthResponse auth)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        return client;
    }
}
