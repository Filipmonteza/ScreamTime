using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    // Movies are soft-deleted, so the inherited reads return active titles only.
    // Admin flows that need a deactivated title ask for it through this overload.
    // Pass includeInactive by name at the call site - it is deliberately required,
    // which is what keeps it from colliding with the inherited GetByIdAsync.
    Task<Movie?> GetByIdAsync(
        Guid id,
        bool includeInactive,
        bool track = false,
        CancellationToken cancellationToken = default);

    // Tracked on purpose: the returned genres get attached to a Movie.
    Task<List<Genre>> GetGenresByIdsAsync(List<Guid> genreIds);
}
