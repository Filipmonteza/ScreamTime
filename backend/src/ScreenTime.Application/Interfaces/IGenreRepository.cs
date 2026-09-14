using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

// Genres are read-only for now, so the inherited operations are all of it.
public interface IGenreRepository : IRepository<Genre>
{
}
