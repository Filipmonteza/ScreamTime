using System.ComponentModel.DataAnnotations;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Contracts;

// Admin/staff search: requires a search term so the endpoint never loads
// every booking in the system. MinLength(2) keeps a single stray keystroke
// from matching everything.
public record BookingSearchDto(
    [Required, MinLength(2)] string Search,
    int Page = 1,
    int PageSize = 12
);

// One booking as the admin search list shows it: enough to identify it and
// see what was bought, without pulling in every field on Booking/Ticket.
public record BookingSummaryDto(
    Guid BookingId,
    string BookingReference,
    string CustomerEmail,
    string MovieTitle,
    string AuditoriumName,
    DateTime ScreeningStartTime,
    List<BookingSeatDto> Seats,
    BookingStatus Status
);

public record BookingSeatDto(
    string RowLabel, 
    int SeatNumber, 
    decimal Price
);