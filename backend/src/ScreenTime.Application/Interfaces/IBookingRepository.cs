using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// Defines the data operations required for storing and retrieving bookings.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Retrieves a booking by its unique ID.
    /// </summary>
    /// <param name="bookingId">The ID of the booking to retrieve.</param>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    /// <returns>
    /// The matching booking, or null if no booking was found.
    /// </returns>
    Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all bookings belonging to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    /// <returns>A read-only list of the user's bookings.</returns>
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the selected seats are available for a screening.
    /// </summary>
    /// <param name="screeningId">The ID of the screening.</param>
    /// <param name="seatIds">The IDs of the selected seats.</param>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    /// <returns>
    /// True if all selected seats are available; otherwise, false.
    /// </returns>
    Task<bool> AreSeatsAvailableAsync(
        Guid screeningId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new booking to the data store.
    /// </summary>
    /// <param name="booking">The booking to add.</param>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Saves all pending booking changes to the data store.
    /// </summary>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}