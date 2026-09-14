using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class GenreRepository : Repository<Genre>, IGenreRepository
{
    public GenreRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    public override async Task<List<Genre>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Query()
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
}
