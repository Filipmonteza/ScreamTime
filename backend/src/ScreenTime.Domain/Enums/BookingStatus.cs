namespace ScreenTime.Domain.Enums;

// Payment state of a booking. Seat availability is derived from
// Ticket.ReleasedAt and never from this enum, so adding a payment step later
// does not touch the availability logic.
public enum BookingStatus
{
    // Seats held but not yet paid for. Unused today: there is no payment step,
    // so bookings are created as Paid. Kept as the hook for one - using it
    // needs a hold expiry, or an abandoned checkout blocks the seat for good.
    Reserved = 0,

    Paid = 1,
    Cancelled = 2
}