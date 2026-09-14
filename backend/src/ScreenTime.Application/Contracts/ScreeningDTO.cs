namespace ScreenTime.Application.Contracts;

// One screening, as both the public programme and the admin list show it. The
// IDs sit next to the names so a caller can link straight to the film or the
// auditorium instead of matching on strings.
public record ScreeningDto(
    Guid ScreeningId,
    Guid MovieId,
    string MovieTitle,
    Guid AuditoriumId,
    string AuditoriumName,
    DateTime StartTime,
    DateTime EndTime,
    bool IsCancelled,
    int Capacity,
    int BookedSeats
);

// EndTime is deliberately not accepted from the caller: it follows from the
// film's runtime plus the cleaning allowance, and letting staff type it by hand
// is how a screening ends up claiming to be shorter than its own film.
public record CreateScreeningDto(
    Guid MovieId,
    Guid AuditoriumId,
    DateTime StartTime
);

public record UpdateScreeningDto(
    Guid MovieId,
    Guid AuditoriumId,
    DateTime StartTime
);

// The programme filter - genre, date and auditorium. Every field is optional.
public record ScreeningFilterDto(
    Guid? MovieId = null,
    Guid? AuditoriumId = null,
    Guid? GenreId = null,
    DateOnly? Date = null,

    // Cancelled and finished screenings are noise on the programme, so they are
    // left out unless a staff view asks for them.
    bool IncludeCancelled = false,
    bool IncludePast = false
);
