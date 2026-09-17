using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// Defines the data operations required for storing and retrieving bookings.
/// The inherited reads load the full booking graph - Screening.Movie,
/// Screening.Auditorium, Tickets and Ticket.Seat.SeatType - because the service
/// reads all of those and there is no lazy loading. Pass track: true when the
/// booking is about to be changed, as cancellation does.
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    // Retrieves all bookings belonging to a specific user, with the same object
    // graph loaded as GetByIdAsync.
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // Determines whether the selected seats are available for a screening.
    // A released ticket (Ticket.ReleasedAt) does not occupy its seat.
    Task<bool> AreSeatsAvailableAsync(
        Guid screeningId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default
    );

    // Gets a screening by its ID, with Movie and Auditorium loaded.
    Task<Screening?> GetScreeningAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default
    );

    // Gets selected seats from a specific auditorium, with SeatType loaded.
    Task<IReadOnlyList<Seat>> GetSeatsByIdsAsync(
        Guid auditoriumId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default
    );

    // checks if a booking reference already exsist
    Task<bool> BookingReferenceExistsAsync(
        string bookingReference,
        CancellationToken cancellationToken = default
    );

    // Gets all seats in an auditorium, with SeatType loaded.
    Task<IReadOnlyList<Seat>> GetSeatsByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default
    );

    // Gets the seat IDs held by an active (not released) ticket for the
    // given screening.
    Task<IReadOnlyCollection<Guid>> GetUnavailableSeatIdsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default
    );

    // Filtered by BookingReference substring or an exact UserId (resolved from
    // an email by BookingService, since Application cannot look up Identity
    // users directly). Paginated in the query itself.
    Task<(List<Booking> Items, int TotalCount)> SearchAsync(
        string search,
        Guid? matchedUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
     );
}
