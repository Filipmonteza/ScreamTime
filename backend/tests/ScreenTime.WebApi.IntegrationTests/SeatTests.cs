using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Adding seats of both types to an auditorium. There is no removal endpoint: a
/// seat that has sold a ticket cannot disappear without taking the ticket's
/// history with it, so closing the salong covers that case instead.
/// </summary>
public class SeatTests : IClassFixture<ScreenTimeApiFactory>
{
    private readonly ScreenTimeApiFactory _factory;

    public SeatTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddSeats_OfBothTypes_ShowsThemOnTheSalongsSeatMap()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var salong = await admin.CreateAuditoriumAsync("Salong Blandad");

        var seatTypes = (await admin.GetFromJsonAsync<List<SeatTypeDto>>("/api/seats/types"))!;
        var standard = seatTypes.Single(t => t.Name == "Standard");
        var vip = seatTypes.Single(t => t.Name == "VIP");

        var response = await admin.PostAsJsonAsync(
            $"/api/auditoriums/{salong.AuditoriumId}/seats",
            new AddSeatsRequestDto(
            [
                new SeatCreateDto("A", 1, standard.Id),
                new SeatCreateDto("A", 2, standard.Id),
                new SeatCreateDto("B", 1, vip.Id),
            ]));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await admin.GetAuditoriumDetailsAsync(salong.AuditoriumId);

        details.Seats.Should().HaveCount(3);
        details.Seats.Should().Contain(s =>
            s.RowLabel == "A" && s.SeatNumber == 1 && s.SeatTypeName == "Standard");
        details.Seats.Should().Contain(s =>
            s.RowLabel == "B" && s.SeatNumber == 1 && s.SeatTypeName == "VIP");

        // Capacity is the seat count, never a number anybody types.
        details.Capacity.Should().Be(3);

        // A booking total is priced off these, so the two types must differ.
        vip.Price.Should().BeGreaterThan(standard.Price);
    }
}
