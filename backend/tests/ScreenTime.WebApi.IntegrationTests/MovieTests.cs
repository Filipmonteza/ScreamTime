using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using ScreenTime.Domain.Enums;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Removing a film. It is a soft delete: the film leaves the catalogue, but
/// the row survives so screenings already scheduled for it keep working.
/// </summary>
public class MovieTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public MovieTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Delete_TakesTheFilmOffTheCatalogue()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var created = await CreateMovieAsync(admin, "Film Borttagen");

        var response = await admin.DeleteAsync($"/api/movies/{created.MovieId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var catalogue = await _factory.CreateClient()
            .GetFromJsonAsync<List<MovieDto>>("/api/movies");

        catalogue.Should().NotContain(m => m.MovieId == created.MovieId);

        // Gone for staff too: no read reaches past the soft delete, only an edit.
        var reloaded = await admin.GetAsync($"/api/movies/{created.MovieId}");
        reloaded.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<MovieDto> CreateMovieAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync(
            "/api/movies",
            new CreateMovieDto(
                title,
                AgeRating.Age11,
                RuntimeMinutes: 100,
                Description: "Testfilm.",
                GenreIds: []));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
    }
}
