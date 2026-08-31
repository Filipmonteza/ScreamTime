using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

// Defines the application operations available for managing bookings.
public interface IBookingService
{
    /// <summary>
    /// Creates a booking for the specified user and selected seats.
    /// </summary>
    /// <param name="userId">The ID of the authenticated user.</param>
    /// <param name="request">The screening and seats selected by the user.</param>
    /// <param name="cancellationToken">
    /// Allows the operation to be cancelled.
    /// </param>
    /// <returns>The newly created booking.</returns>
    Task<BookingDto> CreateBookingAsync(
        Guid userId,
        CreateBookingDto request,
        CancellationToken cancellationToken = default
    );
    
    //Todo aktivera detta i infrastrucktur.
}