using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// Defines the data operations required for storing and retrieving bookings.
/// </summary>
public interface IBookingRepository
{
    
    // Retrieves a booking by its unique ID, tracked, with Screening.Movie,
    // Screening.Auditorium, Tickets and Ticket.Seat.SeatType loaded. The
    // service reads all of those and there is no lazy loading.
    Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default
    );
    
    // Retrieves all bookings belonging to a specific user, with the same
    // object graph loaded as GetByIdAsync.
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
    
    // Adds a new booking to the data store.
    Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default
    );
    
    // Saves all pending booking changes to the data store. A seat taken by
    // someone else between the availability check and the insert surfaces as
    // SeatsUnavailableException rather than a raw DbUpdateException.
    Task SaveChangesAsync(
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
}