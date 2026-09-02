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
            .AsNoTracking()
            .ToListAsync();

    public async Task<Movie?> GetByIdAsync(Guid id, bool includeInactive = false, bool track = false)
    {
        var query = _context.Movies.Include(m => m.Genres).Where(m => m.Id == id);

        if (!includeInactive)
            query = query.Where(m => m.IsActive);

        if (!track)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

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