using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly ScreenTimeDbContext _context;

    public GenreRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genre>> GetAllAsync() =>
        await _context.Genres.OrderBy(g => g.Name).ToListAsync();
}