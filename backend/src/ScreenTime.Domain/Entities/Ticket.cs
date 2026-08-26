using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Ticket : Entity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;

    public Guid SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public decimal Price { get; set; }
}