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

// Represents booking information returned to the client.
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

// Represents bookedSeats
public record BookedSeatDto(
    Guid SeatId,
    string RowLabel,
    int SeatNumber,
    string SeatType,
    decimal SeatPrice
);

// Represents BookingConfirmation
public record BookingConfirmationDto(
    Guid BookingId,
    Guid ScreeningId,
    string BookingReference,
    BookingStatus Status,
    string MovieTitle,
    DateTime ScreeningStartTime,
    string AuditoriumName,
    IReadOnlyList<BookedSeatDto> Seats,
    decimal TotalAmount,
    DateTime CreatedAt
);

// Represents a booking shown on the customer's booking page
public record BookingListItemDto(
    Guid BookingId,
    Guid ScreeningId,
    string BookingReference,
    BookingStatus Status,
    string MovieTitle,
    DateTime ScreeningStartTime,
    string AuditoriumName,
    IReadOnlyList<BookedSeatDto> Seats,
    decimal TotalAmount
);