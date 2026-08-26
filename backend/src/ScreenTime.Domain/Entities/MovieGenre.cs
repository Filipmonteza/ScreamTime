using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class MovieGenre : Entity
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}