using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Movie : Entity
{
    public string Title { get; set; } = string.Empty;
    public int AgeRating { get; set; }
    public int Runtime { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}