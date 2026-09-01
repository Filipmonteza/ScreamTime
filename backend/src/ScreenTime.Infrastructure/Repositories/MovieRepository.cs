using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ScreenTimeDbContext _context;

    public MovieRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Movie>> GetAllAsync() =>
        await _context.Movies
            .Include(m => m.Genres)
            .Where(m => m.IsActive)
            .ToListAsync();

    public async Task<Movie?> GetByIdAsync(Guid id) =>
        await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Genre>> GetGenresByIdsAsync(List<Guid> genreIds) =>
        await _context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync();

    public async Task AddAsync(Movie movie) =>
        await _context.Movies.AddAsync(movie);

    public void Update(Movie movie) =>
        _context.Movies.Update(movie);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}