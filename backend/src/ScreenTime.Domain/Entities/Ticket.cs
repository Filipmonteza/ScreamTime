using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;

    public Guid SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public decimal Price { get; set; }
    
    public DateTime? ReleasedAt { get; private set; }

    // Checks if the ticket is still active
    public bool IsActive => ReleasedAt is null;

    // Releases the ticket so the seat can become available again
    public void Release(DateTime releasedAt)
    {
        if (ReleasedAt is not null)
        {
            throw new InvalidOperationException(
                "Biljetten har redan frisläppts.");
        }
        
        // Sets the time when the ticket was released
        ReleasedAt = releasedAt;
    }
}