using ScreenTime.Domain.Common;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;

    public string BookingReference { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Reserved;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<Ticket> Tickets { get; set; }
        = new List<Ticket>();
}