using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

public class SeatRepository : Repository<Seat>, ISeatRepository
{
    public SeatRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    // SeatType carries the price, which every seat DTO needs.
    protected override IQueryable<Seat> Query(bool track = false) =>
        base.Query(track).Include(s => s.SeatType);

    public async Task<List<Seat>> GetByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(s => s.AuditoriumId == auditoriumId)
            .ToListAsync(cancellationToken);

    public async Task<List<SeatType>> GetSeatTypesByIdsAsync(
        List<Guid> seatTypeIds,
        CancellationToken cancellationToken = default) =>
        await Context.SeatTypes
            .Where(st => seatTypeIds.Contains(st.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<List<SeatType>> GetAllSeatTypesAsync(
        CancellationToken cancellationToken = default) =>
        await Context.SeatTypes
            .OrderBy(st => st.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
