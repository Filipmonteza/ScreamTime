using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class SeatType : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
