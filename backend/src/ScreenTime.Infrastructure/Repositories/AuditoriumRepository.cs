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

    public async Task<List<Auditorium>> GetAllAsync(bool includeInactive = false) =>
        await _context.Auditoriums
            .Where(a => includeInactive || a.IsActive)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Auditorium?> GetByIdAsync(Guid id, bool includeInactive = false, bool track = false)
    {
        var query = track
            ? _context.Auditoriums
            : _context.Auditoriums.AsNoTracking();

        return await query
            .Where(a => a.Id == id && (includeInactive || a.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<List<Seat>> GetSeatsByAuditoriumIdAsync(Guid auditoriumId)
    {
        return await _context.Seats
            .Where(s => s.AuditoriumId == auditoriumId)
            .Include(s => s.SeatType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetCapacityAsync(Guid auditoriumId)
    {
        return await _context.Seats
            .CountAsync(s => s.AuditoriumId == auditoriumId);
    }

    public async Task<Dictionary<Guid, int>> GetCapacitiesAsync(IEnumerable<Guid> auditoriumIds)
    {
        return await _context.Seats
            .Where(s => auditoriumIds.Contains(s.AuditoriumId))
            .GroupBy(s => s.AuditoriumId)
            .Select(g => new { AuditoriumId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AuditoriumId, x => x.Count);
    }

    public async Task<bool> AuditoriumNameExistsAsync(string name, Guid? excludeAuditoriumId = null)
    {
        return await _context.Auditoriums
            .AnyAsync(a =>
                a.Name.ToLower() == name.ToLower() &&
                (!excludeAuditoriumId.HasValue || a.Id != excludeAuditoriumId.Value));
    }

    public async Task AddAsync(Auditorium auditorium)
    {
        await _context.Auditoriums.AddAsync(auditorium);
    }

    public void Update(Auditorium auditorium)
    {
        _context.Auditoriums.Update(auditorium);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}