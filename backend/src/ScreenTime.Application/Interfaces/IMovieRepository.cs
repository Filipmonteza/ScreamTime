using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(Guid id);
    Task<List<Genre>> GetGenresByIdsAsync(List<Guid> genreIds);
    Task AddAsync(Movie movie);
    void Update(Movie movie);
    Task<bool> SaveChangesAsync();
}