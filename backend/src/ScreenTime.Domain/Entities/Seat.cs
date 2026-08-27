using ScreenTime.Domain.Common;
using ScreenTime.Domain.Enums;
using System.Collections.Generic;

namespace ScreenTime.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid AuditoriumId { get; set; }

    public int RowLabel { get; set; }

    public int SeatNumber { get; set; }

    public Auditorium Auditorium { get; set; } = null!;

    public SeatType SeatType { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
