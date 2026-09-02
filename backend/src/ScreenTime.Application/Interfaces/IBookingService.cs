using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

// Defines the application operations available for managing bookings.
/// Creates a booking for the specified user and selected seats.
public interface IBookingService
{
    Task<BookingConfirmationDto> CreateBookingAsync(
        Guid userId,
        CreateBookingDto request,
        CancellationToken cancellationToken = default
    );
    
}