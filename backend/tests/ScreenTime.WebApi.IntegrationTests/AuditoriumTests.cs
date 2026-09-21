using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Creating, editing and removing an auditorium, each with valid and invalid data.
/// Invalid means a duplicate name: the rule the service enforces itself.
/// </summary>
public class AuditoriumTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public AuditoriumTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    // --- Create ---

    [Fact]
    public async Task Create_AsAnAdmin_AddsAnEmptySalongToTheList()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/auditoriums",
            new AuditoriumCreateDto("Salong Nord"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = (await response.Content.ReadFromJsonAsync<AuditoriumDto>())!;
        created.Name.Should().Be("Salong Nord");
        created.IsActive.Should().BeTrue();

        // Capacity is counted from the seats, so a new salong lists as 0.
        var listed = await client.GetAuditoriumsAsync();
        listed.Should()
            .ContainSingle(a => a.AuditoriumId == created.AuditoriumId)
            .Which.Capacity.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithANameAnotherSalongAlreadyHas_IsRejected()
    {
        var client = await _factory.CreateAdminClientAsync();

        await client.CreateAuditoriumAsync("Salong Syd");

        var duplicate = await client.PostAsJsonAsync(
            "/api/auditoriums",
            new AuditoriumCreateDto("Salong Syd"));

        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Edit ---

    [Fact]
    public async Task Update_RenamesTheSalong()
    {
        var client = await _factory.CreateAdminClientAsync();
        var salong = await client.CreateAuditoriumAsync("Salong Innan");

        var response = await client.PutAsJsonAsync(
            $"/api/auditoriums/{salong.AuditoriumId}",
            new AuditoriumUpdateDto("Salong Efter", IsActive: true));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read it back: the 204 alone does not prove the name was written.
        var reloaded = await client.GetAuditoriumDetailsAsync(salong.AuditoriumId);

        reloaded.Name.Should().Be("Salong Efter");
        reloaded.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ToANameAnotherSalongAlreadyHas_IsRejected()
    {
        var client = await _factory.CreateAdminClientAsync();
        var taken = await client.CreateAuditoriumAsync("Salong Upptagen");
        var salong = await client.CreateAuditoriumAsync("Salong Ledig");

        var response = await client.PutAsJsonAsync(
            $"/api/auditoriums/{salong.AuditoriumId}",
            new AuditoriumUpdateDto(taken.Name, IsActive: true));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Remove ---

    [Fact]
    public async Task Delete_AnActiveSalong_TakesItOffTheListWithoutLosingIt()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var salong = await admin.CreateAuditoriumAsync("Salong Stangd");

        var response = await admin.DeleteAsync($"/api/auditoriums/{salong.AuditoriumId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Gone from the public list, and from the public details endpoint.
        var anonymous = _factory.CreateClient();

        (await anonymous.GetAuditoriumsAsync())
            .Should().NotContain(a => a.AuditoriumId == salong.AuditoriumId);

        var publicDetails = await anonymous.GetAsync(
            $"/api/auditoriums/{salong.AuditoriumId}");

        publicDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Still there for staff, inactive: the row keeps its seats and tickets.
        var forStaff = await admin.GetAuditoriumsAsync("?includeInactive=true");

        forStaff.Should()
            .ContainSingle(a => a.AuditoriumId == salong.AuditoriumId)
            .Which.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_AnUnknownSalong_IsNotFound()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.DeleteAsync($"/api/auditoriums/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
