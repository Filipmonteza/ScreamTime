using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ScreenTime.Application.Contracts;
using ScreenTime.Domain.Enums;
using Xunit;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Scenario-based tests for the screening flow: browsing the programme, and the
/// rules that guard scheduling. Every test runs against the real API and the
/// seeded programme, so the overlap rule is checked against screenings that are
/// actually in the database.
/// </summary>
public class ScreeningTests : IClassFixture<ScreenTimeApiFactory>
{
    // The cleaning time ScreeningService adds after the film itself.
    private const int CleaningMinutes = 20;

    // The seeded programme covers the next fourteen days. These tests schedule
    // far past it, and each test picks its own day, so they cannot collide with
    // the seed data or with each other.
    private static readonly DateTime FarFuture = DateTime.Today.AddDays(200);

    private readonly ScreenTimeApiFactory _factory;

    public ScreeningTests(ScreenTimeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_Anonymously_ReturnsTheUpcomingProgramme()
    {
        var client = _factory.CreateClient();

        var screenings = await GetScreeningsAsync(client);

        screenings.Should().NotBeEmpty();
        screenings.Should().OnlyContain(s => !s.IsCancelled);
        screenings.Should().BeInAscendingOrder(s => s.StartTime);
        screenings.Should().OnlyContain(s => s.StartTime > DateTime.Now);
    }

    [Fact]
    public async Task GetAll_AskingForCancelledScreenings_IsRefusedForACustomer()
    {
        var client = await CreateCustomerClientAsync();

        var response = await client.GetAsync("/api/screenings?includeCancelled=true");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithoutAToken_IsUnauthorized()
    {
        var client = _factory.CreateClient();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, auditorium.AuditoriumId, FarFuture.AddHours(18)));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsACustomer_IsForbidden()
    {
        var client = await CreateCustomerClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, auditorium.AuditoriumId, FarFuture.AddHours(18)));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsAnAdmin_DerivesEndTimeFromTheFilmsRuntime()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var startTime = FarFuture.AddHours(18);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, auditorium.AuditoriumId, startTime));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ScreeningDto>();
        created.Should().NotBeNull();
        created!.MovieTitle.Should().Be(movie.Title);
        created.AuditoriumName.Should().Be(auditorium.Name);
        created.StartTime.Should().Be(startTime);
        created.EndTime.Should().Be(
            startTime.AddMinutes(movie.RuntimeMinutes + CleaningMinutes));
        created.IsCancelled.Should().BeFalse();
    }

    // The overlap rule in the three positions a clash can take: a screening
    // that runs into the start of an existing one, one that runs out of its
    // end, and one that sits entirely inside it. Each case is scheduled against
    // a screening that is really in the database, so it is the query in
    // ScreeningRepository.HasOverlapAsync being checked and not a stand-in.

    [Fact]
    public async Task Create_RunningIntoTheStartOfAnExistingScreening_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var existing = FarFuture.AddDays(90).AddHours(18);

        await CreateScreeningAsync(client, movie.MovieId, auditorium.AuditoriumId, existing);

        // Begins half an hour earlier, so its film is still running when the
        // existing screening is due to start.
        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                auditorium.AuditoriumId,
                existing.AddMinutes(-30)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_RunningOutOfTheEndOfAnExistingScreening_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var existing = FarFuture.AddDays(100).AddHours(18);

        await CreateScreeningAsync(client, movie.MovieId, auditorium.AuditoriumId, existing);

        // Begins half an hour before the existing screening has finished.
        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                auditorium.AuditoriumId,
                existing.AddMinutes(SlotMinutes(movie) - 30)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_FallingEntirelyInsideAnExistingScreening_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var movies = await GetMoviesAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        // A short film inside a long one: the only way a screening can start
        // after another begins and still finish before it ends.
        var longest = movies.OrderByDescending(m => m.RuntimeMinutes).First();
        var shortest = movies.OrderBy(m => m.RuntimeMinutes).First();

        var margin = (SlotMinutes(longest) - SlotMinutes(shortest)) / 2;
        margin.Should().BePositive("the seeded films have to differ in length for this case to mean anything");

        var existing = FarFuture.AddDays(110).AddHours(18);

        await CreateScreeningAsync(client, longest.MovieId, auditorium.AuditoriumId, existing);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                shortest.MovieId,
                auditorium.AuditoriumId,
                existing.AddMinutes(margin)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_AtExactlyTheSameTimeAsAnExistingScreening_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var existing = FarFuture.AddDays(120).AddHours(18);

        await CreateScreeningAsync(client, movie.MovieId, auditorium.AuditoriumId, existing);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, auditorium.AuditoriumId, existing));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // The two boundaries. Intervals are half-open, so back-to-back screenings
    // are the normal way to fill a salong and must not be refused - which is
    // also what proves the rule above is not simply rejecting everything.

    [Fact]
    public async Task Create_StartingExactlyWhenTheOneBeforeItEnds_IsAllowed()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var existing = FarFuture.AddDays(130).AddHours(18);

        await CreateScreeningAsync(client, movie.MovieId, auditorium.AuditoriumId, existing);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                auditorium.AuditoriumId,
                existing.AddMinutes(SlotMinutes(movie))));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_EndingExactlyWhenTheOneAfterItBegins_IsAllowed()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var existing = FarFuture.AddDays(140).AddHours(18);

        await CreateScreeningAsync(client, movie.MovieId, auditorium.AuditoriumId, existing);

        // Finishes on the exact minute the existing screening starts.
        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                auditorium.AuditoriumId,
                existing.AddMinutes(-SlotMinutes(movie))));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_AtTheSameTimeInAnotherSalong_IsAllowed()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var first = await GetAuditoriumAsync(client, index: 0);
        var second = await GetAuditoriumAsync(client, index: 1);
        var startTime = FarFuture.AddDays(20).AddHours(18);

        var inFirst = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, first.AuditoriumId, startTime));

        inFirst.StatusCode.Should().Be(HttpStatusCode.Created);

        var inSecond = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, second.AuditoriumId, startTime));

        inSecond.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_InThePast_IsRejected()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                movie.MovieId,
                auditorium.AuditoriumId,
                DateTime.Today.AddDays(-1).AddHours(18)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ForAnUnknownFilm_IsNotFound()
    {
        var client = await CreateAdminClientAsync();
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(
                Guid.NewGuid(),
                auditorium.AuditoriumId,
                FarFuture.AddDays(30).AddHours(18)));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_MovesTheScreeningToAnotherFilmAndTime()
    {
        var client = await CreateAdminClientAsync();
        var movies = await GetMoviesAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var startTime = FarFuture.AddDays(40).AddHours(18);

        var created = await CreateScreeningAsync(
            client,
            movies[0].MovieId,
            auditorium.AuditoriumId,
            startTime);

        var replacement = movies[1];
        var newStartTime = startTime.AddDays(1);

        var response = await client.PutAsJsonAsync(
            $"/api/screenings/{created.ScreeningId}",
            new UpdateScreeningDto(
                replacement.MovieId,
                auditorium.AuditoriumId,
                newStartTime));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Read it back rather than trusting the response body: this is what
        // proves the new film and time were actually written.
        var reloaded = await client.GetFromJsonAsync<ScreeningDto>(
            $"/api/screenings/{created.ScreeningId}");

        reloaded.Should().NotBeNull();
        reloaded!.MovieId.Should().Be(replacement.MovieId);
        reloaded.MovieTitle.Should().Be(replacement.Title);
        reloaded.StartTime.Should().Be(newStartTime);
        reloaded.EndTime.Should().Be(
            newStartTime.AddMinutes(replacement.RuntimeMinutes + CleaningMinutes));
    }

    [Fact]
    public async Task Cancel_TakesTheScreeningOffTheProgrammeAndCannotBeRepeated()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);

        var created = await CreateScreeningAsync(
            client,
            movie.MovieId,
            auditorium.AuditoriumId,
            FarFuture.AddDays(50).AddHours(18));

        var cancelled = await client.PostAsync(
            $"/api/screenings/{created.ScreeningId}/cancel",
            content: null);

        cancelled.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await cancelled.Content.ReadFromJsonAsync<ScreeningDto>();
        body!.IsCancelled.Should().BeTrue();

        var programme = await GetScreeningsAsync(client);
        programme.Should().NotContain(s => s.ScreeningId == created.ScreeningId);

        // Staff can still open it, which is what includeCancelled is for.
        var reloaded = await client.GetFromJsonAsync<ScreeningDto>(
            $"/api/screenings/{created.ScreeningId}?includeCancelled=true");

        reloaded!.IsCancelled.Should().BeTrue();

        var again = await client.PostAsync(
            $"/api/screenings/{created.ScreeningId}/cancel",
            content: null);

        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cancel_FreesTheSlotForANewScreening()
    {
        var client = await CreateAdminClientAsync();
        var movie = await GetFirstMovieAsync(client);
        var auditorium = await GetAuditoriumAsync(client, index: 0);
        var startTime = FarFuture.AddDays(60).AddHours(18);

        var created = await CreateScreeningAsync(
            client,
            movie.MovieId,
            auditorium.AuditoriumId,
            startTime);

        await client.PostAsync($"/api/screenings/{created.ScreeningId}/cancel", content: null);

        // A cancelled screening no longer occupies its salong.
        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movie.MovieId, auditorium.AuditoriumId, startTime));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetAll_FilteredBySalong_ReturnsOnlyThatSalong()
    {
        var client = _factory.CreateClient();
        var auditorium = await GetAuditoriumAsync(client, index: 1);

        var screenings = await GetScreeningsAsync(
            client,
            $"?auditoriumId={auditorium.AuditoriumId}");

        screenings.Should().NotBeEmpty();
        screenings.Should().OnlyContain(s => s.AuditoriumId == auditorium.AuditoriumId);
    }

    [Fact]
    public async Task GetAll_FilteredByDate_ReturnsOnlyThatDay()
    {
        var client = _factory.CreateClient();
        var day = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var screenings = await GetScreeningsAsync(client, $"?date={day:yyyy-MM-dd}");

        screenings.Should().NotBeEmpty();
        screenings.Should().OnlyContain(
            s => DateOnly.FromDateTime(s.StartTime) == day);
    }

    [Fact]
    public async Task GetAll_FilteredByGenre_ReturnsOnlyFilmsInThatGenre()
    {
        var client = _factory.CreateClient();

        var genres = await client.GetFromJsonAsync<List<GenreDto>>("/api/genres");
        var genre = genres!.First();

        var movies = await GetMoviesAsync(client);
        var titlesInGenre = movies
            .Where(m => m.Genres.Contains(genre.Name))
            .Select(m => m.Title)
            .ToList();

        titlesInGenre.Should().NotBeEmpty("the seeded films are linked to genres");

        var screenings = await GetScreeningsAsync(client, $"?genreId={genre.Id}");

        screenings.Should().NotBeEmpty();
        screenings.Should().OnlyContain(s => titlesInGenre.Contains(s.MovieTitle));
    }

    [Fact]
    public async Task Cancel_AlsoCancelsTheBookingsSoldForTheScreening()
    {
        var admin = await CreateAdminClientAsync();
        var customer = await CreateCustomerClientAsync();

        var movie = await GetFirstMovieAsync(admin);
        var auditorium = await GetAuditoriumAsync(admin, index: 1);

        var screening = await CreateScreeningAsync(
            admin,
            movie.MovieId,
            auditorium.AuditoriumId,
            FarFuture.AddDays(70).AddHours(18));

        var booking = await BookTwoSeatsAsync(customer, screening.ScreeningId);
        booking.Status.Should().Be(BookingStatus.Paid);

        var cancelled = await admin.PostAsync(
            $"/api/screenings/{screening.ScreeningId}/cancel",
            content: null);

        cancelled.StatusCode.Should().Be(HttpStatusCode.OK);

        // The customer must not be left holding a ticket to a show that is not
        // happening.
        var reloaded = await customer.GetFromJsonAsync<BookingConfirmationDto>(
            $"/api/bookings/{booking.BookingId}");

        reloaded!.Status.Should().Be(BookingStatus.Cancelled);

        // And it moves out of "mina bokningar" into the history.
        var current = await customer.GetFromJsonAsync<List<BookingListItemDto>>(
            "/api/bookings/current");

        current.Should().NotContain(b => b.BookingId == booking.BookingId);

        var history = await customer.GetFromJsonAsync<List<BookingListItemDto>>(
            "/api/bookings/history");

        history.Should().Contain(b => b.BookingId == booking.BookingId);
    }

    [Fact]
    public async Task CancelBooking_MoreThan15MinutesBeforeTheScreening_IsAllowed()
    {
        var admin = await CreateAdminClientAsync();
        var customer = await CreateCustomerClientAsync();

        var movie = await GetFirstMovieAsync(admin);
        var auditorium = await GetAuditoriumAsync(admin, index: 1);

        // Half an hour out: fifteen minutes on the right side of the deadline.
        var screening = await CreateScreeningAsync(
            admin,
            movie.MovieId,
            auditorium.AuditoriumId,
            DateTime.Now.AddMinutes(30));

        var booking = await BookTwoSeatsAsync(customer, screening.ScreeningId);

        var cancelled = await customer.PostAsync(
            $"/api/bookings/{booking.BookingId}/cancel",
            content: null);

        cancelled.StatusCode.Should().Be(HttpStatusCode.OK);

        var reloaded = await customer.GetFromJsonAsync<BookingConfirmationDto>(
            $"/api/bookings/{booking.BookingId}");

        reloaded!.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task CancelBooking_LessThan15MinutesBeforeTheScreening_IsRefused()
    {
        var admin = await CreateAdminClientAsync();
        var customer = await CreateCustomerClientAsync();

        var movie = await GetFirstMovieAsync(admin);
        var auditorium = await GetAuditoriumAsync(admin, index: 0);
        
        var screening = await CreateScreeningAsync(
            admin,
            movie.MovieId,
            auditorium.AuditoriumId,
            DateTime.Now.AddMinutes(10));

        var booking = await BookTwoSeatsAsync(customer, screening.ScreeningId);

        var cancelled = await customer.PostAsync(
            $"/api/bookings/{booking.BookingId}/cancel",
            content: null);

        cancelled.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // The booking and its seats are untouched.
        var reloaded = await customer.GetFromJsonAsync<BookingConfirmationDto>(
            $"/api/bookings/{booking.BookingId}");

        reloaded!.Status.Should().Be(BookingStatus.Paid);
    }

    [Fact]
    public async Task Update_OfABookedScreening_MayMoveTheTimeButNotTheSalong()
    {
        var admin = await CreateAdminClientAsync();
        var customer = await CreateCustomerClientAsync();

        var movie = await GetFirstMovieAsync(admin);
        var first = await GetAuditoriumAsync(admin, index: 0);
        var second = await GetAuditoriumAsync(admin, index: 1);
        var startTime = FarFuture.AddDays(80).AddHours(18);

        var screening = await CreateScreeningAsync(
            admin,
            movie.MovieId,
            first.AuditoriumId,
            startTime);

        await BookTwoSeatsAsync(customer, screening.ScreeningId);

        // A sold ticket names a seat in this salong, so the screening cannot
        // move to another one.
        var moved = await admin.PutAsJsonAsync(
            $"/api/screenings/{screening.ScreeningId}",
            new UpdateScreeningDto(movie.MovieId, second.AuditoriumId, startTime));

        moved.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Rescheduling it within the same salong is fine - the tickets still
        // point at the same screening and the same seats.
        var rescheduled = await admin.PutAsJsonAsync(
            $"/api/screenings/{screening.ScreeningId}",
            new UpdateScreeningDto(
                movie.MovieId,
                first.AuditoriumId,
                startTime.AddHours(2)));

        rescheduled.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Books the first two free seats on a screening, the way the seat picker
    // will once the booking UI exists.
    private static async Task<BookingConfirmationDto> BookTwoSeatsAsync(
        HttpClient client,
        Guid screeningId)
    {
        var seatMap = await client.GetFromJsonAsync<ScreeningSeatMapDto>(
            $"/api/bookings/screenings/{screeningId}/seats");

        var seatIds = seatMap!.Seats
            .Where(seat => seat.IsAvailable)
            .Take(2)
            .Select(seat => seat.SeatId)
            .ToList();

        seatIds.Should().HaveCount(2);

        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingDto(screeningId, seatIds));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<BookingConfirmationDto>())!;
    }

    private static async Task<ScreeningDto> CreateScreeningAsync(
        HttpClient client,
        Guid movieId,
        Guid auditoriumId,
        DateTime startTime)
    {
        var response = await client.PostAsJsonAsync(
            "/api/screenings",
            new CreateScreeningDto(movieId, auditoriumId, startTime));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<ScreeningDto>())!;
    }

    private static async Task<List<ScreeningDto>> GetScreeningsAsync(
        HttpClient client,
        string query = "")
    {
        var screenings = await client.GetFromJsonAsync<List<ScreeningDto>>(
            $"/api/screenings{query}");

        return screenings!;
    }

    // The slot a screening occupies: the film itself plus the cleaning time
    // ScreeningService adds after it.
    private static int SlotMinutes(MovieDto movie) =>
        movie.RuntimeMinutes + CleaningMinutes;

    private static async Task<List<MovieDto>> GetMoviesAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<List<MovieDto>>("/api/movies"))!;

    private static async Task<MovieDto> GetFirstMovieAsync(HttpClient client) =>
        (await GetMoviesAsync(client)).First();

    private static async Task<AuditoriumListDto> GetAuditoriumAsync(
        HttpClient client,
        int index)
    {
        var auditoriums = await client.GetFromJsonAsync<List<AuditoriumListDto>>(
            "/api/auditoriums");

        return auditoriums![index];
    }

    private Task<HttpClient> CreateAdminClientAsync() =>
        CreateAuthenticatedClientAsync("admin@screentime.com", "Admin1234!");

    private Task<HttpClient> CreateCustomerClientAsync() =>
        CreateAuthenticatedClientAsync("test@screentime.com", "Test1234!");

    // Both accounts come from UserSeeder, which runs in the test host exactly as
    // it does in development.
    private async Task<HttpClient> CreateAuthenticatedClientAsync(
        string email,
        string password)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }
}
