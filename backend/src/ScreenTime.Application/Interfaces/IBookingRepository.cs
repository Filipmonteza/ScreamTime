using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// Defines the data operations required for storing and retrieving bookings.
/// </summary>
public interface IBookingRepository
{
    
    // Retrieves a booking by its unique ID.
    Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default
    );
    
    // Retrieves all bookings belonging to a specific user.
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    
    /// Determines whether the selected seats are available for a screening.
    Task<bool> AreSeatsAvailableAsync(
        Guid screeningId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default
    );
    
    /// Adds a new booking to the data store.
    Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default
    );
    
    // Saves all pending booking changes to the data store.
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
    
    Task<Screening?> GetScreeningAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Seat>> GetSeatsByIdsAsync(
        Guid auditoriumId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default
    );

    Task<bool> BookingReferenceExistsAsync(
        string bookingReference,
        CancellationToken cancellationToken = default
    );
}