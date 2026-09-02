using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

// Defines the application operations available for managing bookings.
/// Creates a booking for the specified user and selected seats.
public interface IBookingService
{
    // Booking confirmation
    Task<BookingConfirmationDto> CreateBookingAsync(
        Guid userId,
        CreateBookingDto request,
        CancellationToken cancellationToken = default
    );
    
    // Cancels booking
    Task<BookingDto> CancelBookingAsync(
        Guid bookingId,
        Guid requesterUserId,
        bool canManageAllBookings,
        CancellationToken cancellationToken = default
    );
    
    // Gets all current bookings for a user
    Task<IReadOnlyList<BookingListItemDto>> GetCurrentBookingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    
   
}