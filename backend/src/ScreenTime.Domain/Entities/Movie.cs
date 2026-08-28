using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int AgeRating { get; set; }
    public int RuntimeMinutes { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}