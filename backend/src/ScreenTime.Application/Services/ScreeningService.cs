using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Services;

public class ScreeningService : IScreeningService
{
    // Time to empty and clean the auditorium before the next audience comes in.
    // The seed data schedules with the same allowance.
    private const int CleaningMinutes = 20;

    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IAuditoriumRepository _auditoriumRepository;

    // A screening is a film in an auditorium, so scheduling one has to read both.
    public ScreeningService(
        IScreeningRepository screeningRepository,
        IMovieRepository movieRepository,
        IAuditoriumRepository auditoriumRepository)
    {
        _screeningRepository = screeningRepository;
        _movieRepository = movieRepository;
        _auditoriumRepository = auditoriumRepository;
    }

    public async Task<List<ScreeningDto>> GetScreeningsAsync(
        ScreeningFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var screenings = await _screeningRepository.GetFilteredAsync(
            filter,
            cancellationToken);

        return screenings.Select(MapToDto).ToList();
    }

    public async Task<ScreeningDto> GetScreeningByIdAsync(
        Guid id,
        bool includeCancelled = false,
        CancellationToken cancellationToken = default)
    {
        var screening = await _screeningRepository.GetByIdAsync(
            id,
            includeCancelled,
            cancellationToken: cancellationToken);

        if (screening is null)
        {
            throw new KeyNotFoundException("Visningen kunde inte hittas.");
        }

        return MapToDto(screening);
    }

    public async Task<ScreeningDto> CreateScreeningAsync(
        CreateScreeningDto dto,
        CancellationToken cancellationToken = default)
    {
        var movie = await GetSchedulableMovieAsync(dto.MovieId, cancellationToken);

        var auditorium = await GetSchedulableAuditoriumAsync(
            dto.AuditoriumId,
            cancellationToken);

        var (startTime, endTime) = PlanSlot(dto.StartTime, movie);

        await EnsureAuditoriumIsFreeAsync(
            auditorium.Id,
            startTime,
            endTime,
            excludeScreeningId: null,
            cancellationToken);

        // Only the foreign keys are set. Assigning the navigation properties
        // would hand EF two untracked entities to insert alongside the
        // screening, which is not what a read of an existing film means.
        var screening = new Screening
        {
            MovieId = movie.Id,
            AuditoriumId = auditorium.Id,
            StartTime = startTime,
            EndTime = endTime,
            IsCancelled = false
        };

        await _screeningRepository.AddAsync(screening, cancellationToken);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(screening, movie.Title, auditorium.Name);
    }

    public async Task<ScreeningDto> UpdateScreeningAsync(
        Guid id,
        UpdateScreeningDto dto,
        CancellationToken cancellationToken = default)
    {
        var screening = await _screeningRepository.GetByIdAsync(
            id,
            includeCancelled: true,
            track: true,
            cancellationToken);

        if (screening is null)
        {
            throw new KeyNotFoundException("Visningen kunde inte hittas.");
        }

        if (screening.IsCancelled)
        {
            throw new InvalidOperationException(
                "En avbokad visning kan inte ändras.");
        }

        if (screening.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "En visning som redan har börjat kan inte ändras.");
        }

        var movie = await GetSchedulableMovieAsync(dto.MovieId, cancellationToken);

        var auditorium = await GetSchedulableAuditoriumAsync(
            dto.AuditoriumId,
            cancellationToken);

        var (startTime, endTime) = PlanSlot(dto.StartTime, movie);

        await EnsureAuditoriumIsFreeAsync(
            auditorium.Id,
            startTime,
            endTime,
            excludeScreeningId: screening.Id,
            cancellationToken);

        // A sold ticket names a seat in one particular auditorium and was bought to
        // see one particular film, so once a screening has bookings only its
        // time may still move.
        var changesFilmOrSalong =
            movie.Id != screening.MovieId ||
            auditorium.Id != screening.AuditoriumId;

        if (changesFilmOrSalong &&
            await _screeningRepository.HasActiveBookingsAsync(
                screening.Id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Visningen har bokningar och kan varken byta film eller salong. " +
                "Avboka visningen och lägg upp en ny.");
        }

        screening.MovieId = movie.Id;
        screening.AuditoriumId = auditorium.Id;
        screening.StartTime = startTime;
        screening.EndTime = endTime;

        _screeningRepository.Update(screening);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        // The screening was loaded with the film and auditorium it had before this
        // call, so the new names have to come from the entities read above.
        return MapToDto(screening, movie.Title, auditorium.Name);
    }

    public async Task<ScreeningDto> CancelScreeningAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var screening = await _screeningRepository.GetByIdAsync(
            id,
            includeCancelled: true,
            track: true,
            cancellationToken);

        if (screening is null)
        {
            throw new KeyNotFoundException("Visningen kunde inte hittas.");
        }

        if (screening.IsCancelled)
        {
            throw new InvalidOperationException("Visningen är redan avbokad.");
        }

        var cancelledAt = DateTime.UtcNow;

        screening.IsCancelled = true;

        // The bookings have to go with it, or a customer keeps tickets to a show
        // that is not happening and still sees it under "mina bokningar".
        // Tickets are released rather than deleted, exactly as a customer's own
        // cancellation does it, so the booking survives as history.
        var bookings = await _screeningRepository.GetActiveBookingsAsync(
            screening.Id,
            cancellationToken);

        foreach (var booking in bookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = cancelledAt;

            foreach (var ticket in booking.Tickets.Where(ticket => ticket.IsActive))
            {
                ticket.Release(cancelledAt);
            }
        }

        _screeningRepository.Update(screening);
        await _screeningRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(screening);
    }

    // An unknown or deactivated film cannot be put on the programme. The
    // repository's default read already leaves deactivated films out.
    private async Task<Movie> GetSchedulableMovieAsync(
        Guid movieId,
        CancellationToken cancellationToken)
    {
        if (movieId == Guid.Empty)
        {
            throw new ArgumentException(
                "Filmens ID får inte vara tomt.",
                nameof(movieId));
        }

        var movie = await _movieRepository.GetByIdAsync(
            movieId,
            cancellationToken: cancellationToken);

        return movie ?? throw new KeyNotFoundException(
            "Filmen kunde inte hittas eller är avaktiverad.");
    }

    // Same again for the auditorium: the default read returns active ones only.
    private async Task<Auditorium> GetSchedulableAuditoriumAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken)
    {
        if (auditoriumId == Guid.Empty)
        {
            throw new ArgumentException(
                "Salongens ID får inte vara tomt.",
                nameof(auditoriumId));
        }

        var auditorium = await _auditoriumRepository.GetByIdAsync(
            auditoriumId,
            cancellationToken: cancellationToken);

        return auditorium ?? throw new KeyNotFoundException(
            "Salongen kunde inte hittas eller är avaktiverad.");
    }

    // The slot a film occupies: its runtime plus the time it takes to clean the
    // auditorium afterwards. EndTime is derived here rather than supplied by the
    // caller, so it can never contradict the film.
    private static (DateTime StartTime, DateTime EndTime) PlanSlot(
        DateTime startTime,
        Movie movie)
    {
        if (startTime <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "En visning måste börja i framtiden.",
                nameof(startTime));
        }

        return (
            startTime,
            startTime.AddMinutes(movie.RuntimeMinutes + CleaningMinutes));
    }

    // The rule ScreeningConfiguration cannot express: one auditorium cannot show two
    // films at once. The index on (AuditoriumId, StartTime) is what makes this
    // cheap to ask.
    private async Task EnsureAuditoriumIsFreeAsync(
        Guid auditoriumId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeScreeningId,
        CancellationToken cancellationToken)
    {
        var overlaps = await _screeningRepository.HasOverlapAsync(
            auditoriumId,
            startTime,
            endTime,
            excludeScreeningId,
            cancellationToken);

        if (overlaps)
        {
            throw new InvalidOperationException(
                "Salongen är upptagen av en annan visning under den tiden.");
        }
    }

    private static ScreeningDto MapToDto(Screening screening) =>
        MapToDto(screening, screening.Movie.Title, screening.Auditorium.Name);

    // For the two cases where the screening's own navigation properties cannot
    // be trusted: a screening that was just created carries only its foreign
    // keys, and one that was just updated still holds the film and auditorium it was
    // loaded with.
    private static ScreeningDto MapToDto(
        Screening screening,
        string movieTitle,
        string auditoriumName) =>
        new(
            screening.Id,
            screening.MovieId,
            movieTitle,
            screening.AuditoriumId,
            auditoriumName,
            screening.StartTime,
            screening.EndTime,
            screening.IsCancelled);
}
