using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class AuditoriumRepository : IAuditoriumRepository
{
    private readonly ScreenTimeDbContext _context;

    public AuditoriumRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Auditorium>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        await _context.Auditoriums
            .Where(a => includeInactive || a.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    public async Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive = false,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = track
            ? _context.Auditoriums
            : _context.Auditoriums.AsNoTracking();

        return await query
            .Where(a => a.Id == id && (includeInactive || a.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetCapacitiesAsync(
        IEnumerable<Guid> auditoriumIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.Seats
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
    }

    public async Task<bool> AuditoriumNameExistsAsync(
        string name,
        Guid? excludeAuditoriumId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Auditoriums
            .AnyAsync(
                a =>
                    a.IsActive &&
                    a.Name == name &&
                    (!excludeAuditoriumId.HasValue ||
                     a.Id != excludeAuditoriumId.Value),
                cancellationToken);
    }

    public async Task AddAsync(
        Auditorium auditorium,
        CancellationToken cancellationToken = default)
    {
        await _context.Auditoriums.AddAsync(
            auditorium,
            cancellationToken);
    }

    public void Update(Auditorium auditorium)
    {
        _context.Auditoriums.Update(auditorium);
    }

    public async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}