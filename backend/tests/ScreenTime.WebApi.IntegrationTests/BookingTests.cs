using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScreenTime.Application.Contracts;
using ScreenTime.Domain.Entities;
using ScreenTime.Domain.Enums;
using ScreenTime.Infrastructure.Data;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Booking seats and the confirmation it returns, plus the rule the flow rests
/// on: one active ticket per seat per screening.
///
/// Double-booking is checked three ways. Through the API is what a customer
/// meets. Against the database is the only thing that proves the filtered
/// unique index holds. Rebooking a released seat covers the
/// index's filter, without which a cancellation would block its seat for good.
/// </summary>
public class BookingTests : IClassFixture<ScreenTimeApiFactory>
{
    // SQLite reports a violated unique index as extended result code 2067.
    private const int SqliteConstraintUnique = 2067;

    // Past the seeded programme, and a day per test, so nothing collides with
    // the programme or with another test. UTC, like everything the API speaks.
    private static readonly DateTime FarFuture = DateTime.UtcNow.Date.AddDays(300);

    private readonly ScreenTimeApiFactory _factory;

    public BookingTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    // --- Booking and its confirmation ---

    [Fact]
    public async Task Book_TheSeatsIPicked_ConfirmsThemWithAReferenceAndATotal()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var customer = await _factory.CreateCustomerClientAsync();

        var screening = await ScheduleScreeningAsync(admin, dayOffset: 0);
        var chosen = await PickFreeSeatsAsync(customer, screening.ScreeningId, count: 2);

        var confirmation = await BookAsync(customer, screening.ScreeningId, chosen);

        // What the confirmation page shows: film, time, salong, seats, price.
        confirmation.MovieTitle.Should().Be(screening.MovieTitle);
        confirmation.AuditoriumName.Should().Be(screening.AuditoriumName);
        confirmation.ScreeningStartTime.Should().Be(screening.StartTime);
        confirmation.Status.Should().Be(BookingStatus.Paid);
        confirmation.Seats.Should().HaveCount(2);
        confirmation.Seats.Select(s => s.SeatId)
            .Should().BeEquivalentTo(chosen.Select(s => s.SeatId));
        confirmation.TotalAmount.Should().Be(chosen.Sum(s => s.Price));
        confirmation.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);

        // Quoted at the counter, so no O/0 or I/1 to mistake for each other.
        confirmation.BookingReference.Should().MatchRegex("^[A-HJ-NP-Z2-9]{10}$");

        // The booking reads back as the same confirmation.
        var reloaded = await customer.GetFromJsonAsync<BookingConfirmationDto>(
            $"/api/bookings/{confirmation.BookingId}");

        reloaded.Should().BeEquivalentTo(confirmation);

        var current = await customer.GetFromJsonAsync<List<BookingListItemDto>>(
            "/api/bookings/current");

        current.Should().Contain(b => b.BookingId == confirmation.BookingId);
    }

    // --- Double booking ---

    [Fact]
    public async Task Book_ASeatAnotherCustomerAlreadyHas_IsRefused()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var first = await _factory.CreateCustomerClientAsync();
        var second = await _factory.RegisterCustomerClientAsync();

        var screening = await ScheduleScreeningAsync(admin, dayOffset: 10);

        var chosen = await PickFreeSeatsAsync(first, screening.ScreeningId, count: 1);
        await BookAsync(first, screening.ScreeningId, chosen);

        // The second customer asks for the very same seat.
        var response = await second.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingDto(
                screening.ScreeningId,
                chosen.Select(s => s.SeatId).ToList()));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // And nothing was written for them: they have no booking at all.
        var theirBookings = await second.GetFromJsonAsync<List<BookingListItemDto>>(
            "/api/bookings/current");

        theirBookings.Should().BeEmpty();
    }

    // The availability check runs before the insert, so it cannot show the
    // database would stop a booking that slipped past it. This writes the
    // second ticket directly: the state two simultaneous bookings produce.
    [Fact]
    public async Task TheDatabase_RefusesASecondActiveTicketForTheSameSeat()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var customer = await _factory.CreateCustomerClientAsync();

        var screening = await ScheduleScreeningAsync(admin, dayOffset: 20);
        var chosen = await PickFreeSeatsAsync(customer, screening.ScreeningId, count: 1);
        var booking = await BookAsync(customer, screening.ScreeningId, chosen);
        var seat = booking.Seats.Single();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();

        db.Tickets.Add(new Ticket
        {
            BookingId = booking.BookingId,
            ScreeningId = screening.ScreeningId,
            SeatId = seat.SeatId,
            Price = seat.SeatPrice
        });

        var thrown = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await db.SaveChangesAsync());

        thrown.InnerException.Should().BeOfType<SqliteException>()
            .Which.SqliteExtendedErrorCode.Should().Be(SqliteConstraintUnique);
    }

    // The index filters on ReleasedAt, so a cancellation gives the seat back.
    [Fact]
    public async Task Book_ASeatReleasedByACancellation_IsAllowedAgain()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var first = await _factory.CreateCustomerClientAsync();
        var second = await _factory.RegisterCustomerClientAsync();

        var screening = await ScheduleScreeningAsync(admin, dayOffset: 30);

        var chosen = await PickFreeSeatsAsync(first, screening.ScreeningId, count: 1);
        var booking = await BookAsync(first, screening.ScreeningId, chosen);

        await SeatShouldBeAvailable(first, screening.ScreeningId, chosen, expected: false);

        var cancelled = await first.PostAsync(
            $"/api/bookings/{booking.BookingId}/cancel",
            content: null);

        cancelled.StatusCode.Should().Be(HttpStatusCode.OK);

        await SeatShouldBeAvailable(first, screening.ScreeningId, chosen, expected: true);

        // Somebody else can now have it.
        var rebooked = await BookAsync(second, screening.ScreeningId, chosen);

        rebooked.Seats.Single().SeatId.Should().Be(chosen.Single().SeatId);
    }

    // A screening on a day of its own, in the small auditorium.
    private async Task<ScreeningDto> ScheduleScreeningAsync(
        HttpClient admin,
        int dayOffset)
    {
        var movie = (await admin.GetFromJsonAsync<List<MovieDto>>("/api/movies"))!.First();
        var salong = (await admin.GetAuditoriumsAsync())
            .Single(a => a.Name == "Lilla salongen");

        var response = await admin.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                salong.AuditoriumId,
                FarFuture.AddDays(dayOffset).AddHours(18)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<ScreeningDto>())!;
    }

    private static async Task<List<SeatAvailabilityDto>> PickFreeSeatsAsync(
        HttpClient client,
        Guid screeningId,
        int count)
    {
        var seatMap = await GetSeatMapAsync(client, screeningId);

        var seats = seatMap.Seats
            .Where(seat => seat.IsAvailable)
            .Take(count)
            .ToList();

        seats.Should().HaveCount(count);

        return seats;
    }

    private static async Task<BookingConfirmationDto> BookAsync(
        HttpClient client,
        Guid screeningId,
        IReadOnlyCollection<SeatAvailabilityDto> seats)
    {
        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingDto(screeningId, seats.Select(s => s.SeatId).ToList()));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<BookingConfirmationDto>())!;
    }

    private static async Task<ScreeningSeatMapDto> GetSeatMapAsync(
        HttpClient client,
        Guid screeningId) =>
        (await client.GetFromJsonAsync<ScreeningSeatMapDto>(
            $"/api/bookings/screenings/{screeningId}/seats"))!;

    private static async Task SeatShouldBeAvailable(
        HttpClient client,
        Guid screeningId,
        IEnumerable<SeatAvailabilityDto> seats,
        bool expected)
    {
        var seatIds = seats.Select(s => s.SeatId).ToHashSet();
        var seatMap = await GetSeatMapAsync(client, screeningId);

        seatMap.Seats
            .Where(seat => seatIds.Contains(seat.SeatId))
            .Should().OnlyContain(seat => seat.IsAvailable == expected);
    }
}
