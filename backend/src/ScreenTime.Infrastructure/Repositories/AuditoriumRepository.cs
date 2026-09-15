using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class AuditoriumRepository : Repository<Auditorium>, IAuditoriumRepository
{
    public AuditoriumRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    public override async Task<List<Auditorium>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await GetAllAsync(includeInactive: false, cancellationToken);

    public async Task<List<Auditorium>> GetAllAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(a => includeInactive || a.IsActive)
            .ToListAsync(cancellationToken);

    public override async Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool track = false,
        CancellationToken cancellationToken = default) =>
        await GetByIdAsync(id, includeInactive: false, track, cancellationToken);

    public async Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive,
        bool track = false,
        CancellationToken cancellationToken = default) =>
        await Query(track)
            .Where(a => a.Id == id && (includeInactive || a.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Dictionary<Guid, int>> GetCapacitiesAsync(
        IEnumerable<Guid> auditoriumIds,
        CancellationToken cancellationToken = default) =>
        await Context.Seats
            .Where(s => auditoriumIds.Contains(s.AuditoriumId))
            .GroupBy(s => s.AuditoriumId)
            .Select(g => new
            {
                AuditoriumId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(
                x => x.AuditoriumId,
                x => x.Count,
                cancellationToken);

    public async Task<bool> AuditoriumNameExistsAsync(
        string name,
        Guid? excludeAuditoriumId = null,
        CancellationToken cancellationToken = default) =>
        await Entities
            .AnyAsync(
                a =>
                    a.IsActive &&
                    a.Name == name &&
                    (!excludeAuditoriumId.HasValue ||
                     a.Id != excludeAuditoriumId.Value),
                cancellationToken);
}
