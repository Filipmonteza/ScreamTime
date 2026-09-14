using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class ScreeningRepository : Repository<Screening>, IScreeningRepository
{
    public ScreeningRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    // A screening is only meaningful together with its movie and its auditorium.
    protected override IQueryable<Screening> Query(bool track = false) =>
        base.Query(track)
            .Include(s => s.Movie)
            .Include(s => s.Auditorium);

    public override async Task<List<Screening>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(s => !s.IsCancelled)
            .ToListAsync(cancellationToken);
}
