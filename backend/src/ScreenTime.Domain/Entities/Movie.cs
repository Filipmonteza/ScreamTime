using ScreenTime.Domain.Common;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public AgeRating AgeRating { get; set; }
    public int RuntimeMinutes { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Poster { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
}