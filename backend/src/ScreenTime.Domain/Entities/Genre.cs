using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}