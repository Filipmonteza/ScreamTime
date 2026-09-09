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

    public async Task<List<Seat>> GetAllAsync() =>
        await _context.Seats
            .Include(s => s.Auditorium)
            .Include(s => s.SeatType)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Seat>> GetByAuditoriumIdAsync(Guid auditoriumId)
    {
        return await _context.Seats
            .Where(s => s.AuditoriumId == auditoriumId)
            .Include(s => s.SeatType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SeatType>> GetSeatTypesByIdsAsync(List<Guid> seatTypeIds)
    {
        return await _context.SeatTypes
            .Where(st => seatTypeIds.Contains(st.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats)
    {
        await _context.Seats.AddRangeAsync(seats);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}