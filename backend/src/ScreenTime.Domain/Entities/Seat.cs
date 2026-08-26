using System.Collections.Generic;
using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Seat : Entity
{
    public int SeatId { get; set; }

    public int AuditoriumId { get; set; }

    public int SeatTypeId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public Auditorium Auditorium { get; set; } = null!;

    public SeatType SeatType { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}