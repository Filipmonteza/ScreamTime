using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly ScreenTimeDbContext _context;

    public SeatRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Seats
            .Include(s => s.Auditorium)
            .Include(s => s.SeatType)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<List<Seat>> GetByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .Where(s => s.AuditoriumId == auditoriumId)
            .Include(s => s.SeatType)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SeatType>> GetSeatTypesByIdsAsync(
        List<Guid> seatTypeIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.SeatTypes
            .Where(st => seatTypeIds.Contains(st.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SeatType>> GetAllSeatTypesAsync(
    CancellationToken cancellationToken = default) =>
    await _context.SeatTypes
        .OrderBy(st => st.Name)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<Seat> seats,
        CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddRangeAsync(
            seats,
            cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}