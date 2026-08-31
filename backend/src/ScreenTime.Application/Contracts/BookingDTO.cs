using System.ComponentModel.DataAnnotations;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Contracts;

// CreateBookingDTO only contains
// The ID of the screening to book
// The IDs of the seats selected by the user
public record CreateBookingDto(
    Guid ScreeningId,
    [Required, MinLength(1)]
    List<Guid> SeatIds
);


/// <summary>
/// Represents booking information returned to the client.
/// </summary>
/// <param name="BookingId">The unique ID of the booking.</param>
/// <param name="ScreeningId">The ID of the booked screening.</param>
/// <param name="BookingReference">
/// The unique reference used to identify the booking.
/// </param>
/// <param name="Status">The current status of the booking.</param>
/// <param name="CreatedAt">The date and time when the booking was created.</param>
/// <param name="CancelledAt">
/// The date and time when the booking was cancelled, or null if it is active.
/// </param>
/// <param name="TotalAmount">The total price of the booking.</param>
/// <param name="TicketCount">The number of tickets included in the booking.</param>
public record BookingDto(
    Guid BookingId,
    Guid ScreeningId,
    string BookingReference,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    decimal TotalAmount,
    int TicketCount
    );