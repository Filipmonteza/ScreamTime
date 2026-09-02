using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(Guid id, bool includeInactive = false, bool track = false);
    Task<List<Genre>> GetGenresByIdsAsync(List<Guid> genreIds);
    Task AddAsync(Movie movie);
    void Update(Movie movie);
    Task<bool> SaveChangesAsync();
}