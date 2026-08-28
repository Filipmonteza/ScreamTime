using System;
using System.Collections.Generic;
using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Screening : BaseEntity
{
    public Guid MovieId { get; set; }

    public Guid AuditoriumId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsCancelled { get; set; }

    public Movie Movie { get; set; } = null!;

    public Auditorium Auditorium { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
