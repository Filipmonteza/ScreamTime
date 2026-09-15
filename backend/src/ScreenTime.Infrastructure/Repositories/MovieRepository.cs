using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    // Every movie read needs its genres; the DTOs project them.
    protected override IQueryable<Movie> Query(bool track = false) =>
        base.Query(track).Include(m => m.Genres);

    public override async Task<List<Movie>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(m => m.IsActive)
            .ToListAsync(cancellationToken);

    public override async Task<Movie?> GetByIdAsync(
        Guid id,
        bool track = false,
        CancellationToken cancellationToken = default) =>
        await GetByIdAsync(id, includeInactive: false, track, cancellationToken);

    public async Task<Movie?> GetByIdAsync(
        Guid id,
        bool includeInactive,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = Query(track).Where(m => m.Id == id);

        if (!includeInactive)
            query = query.Where(m => m.IsActive);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    // Tracked on purpose: these get attached to a Movie by MovieService.
    public async Task<List<Genre>> GetGenresByIdsAsync(List<Guid> genreIds) =>
        await Context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync();
}
