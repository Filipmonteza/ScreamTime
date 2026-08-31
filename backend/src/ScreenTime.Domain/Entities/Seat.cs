using ScreenTime.Domain.Common;
using System.Collections.Generic;

namespace ScreenTime.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid AuditoriumId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public Auditorium Auditorium { get; set; } = null!;

    public Guid SeatTypeId { get; set; }

    public SeatType SeatType { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
