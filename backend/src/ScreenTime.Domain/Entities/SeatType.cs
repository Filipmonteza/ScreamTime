using System.Collections.Generic;
using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class SeatType : Entity
{
    public int SeatTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}