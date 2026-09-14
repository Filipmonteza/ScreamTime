using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Domain.Enums;
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
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

    public override async Task<Screening?> GetByIdAsync(
        Guid id,
        bool track = false,
        CancellationToken cancellationToken = default) =>
        await GetByIdAsync(id, includeCancelled: false, track, cancellationToken);

    public async Task<Screening?> GetByIdAsync(
        Guid id,
        bool includeCancelled,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = Query(track).Where(s => s.Id == id);

        if (!includeCancelled)
            query = query.Where(s => !s.IsCancelled);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Screening>> GetFilteredAsync(
        ScreeningFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = Query();

        if (!filter.IncludeCancelled)
            query = query.Where(s => !s.IsCancelled);

        if (!filter.IncludePast)
        {
            // The same clock BookingService measures against, so "still
            // bookable" means the same thing on both sides.
            var now = DateTime.UtcNow;
            query = query.Where(s => s.StartTime > now);
        }

        if (filter.MovieId is { } movieId)
            query = query.Where(s => s.MovieId == movieId);

        if (filter.AuditoriumId is { } auditoriumId)
            query = query.Where(s => s.AuditoriumId == auditoriumId);

        // Genre belongs to the film, not to the screening, so this reaches
        // through the link table rather than copying the genre onto Screening.
        if (filter.GenreId is { } genreId)
            query = query.Where(s => s.Movie.Genres.Any(g => g.Id == genreId));

        if (filter.Date is { } date)
        {
            // One calendar day, from midnight up to but not including the next.
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);

            query = query.Where(s => s.StartTime >= dayStart && s.StartTime < dayEnd);
        }

        return await query
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Auditorium.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid auditoriumId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeScreeningId = null,
        CancellationToken cancellationToken = default)
    {
        // No Includes: this only ever answers a bool.
        var query = Entities.Where(s => s.AuditoriumId == auditoriumId && !s.IsCancelled);

        if (excludeScreeningId is { } excludedId)
            query = query.Where(s => s.Id != excludedId);

        // Half-open intervals, so a screening that ends exactly as the next one
        // starts is not an overlap.
        return await query.AnyAsync(
            s => s.StartTime < endTime && startTime < s.EndTime,
            cancellationToken);
    }

    public async Task<bool> HasActiveBookingsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default) =>
        await ActiveBookings(screeningId).AnyAsync(cancellationToken);

    // Tracked on purpose, and with the tickets: ScreeningService cancels these
    // bookings and releases their seats.
    public async Task<List<Booking>> GetActiveBookingsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default) =>
        await ActiveBookings(screeningId)
            .Include(b => b.Tickets)
            .ToListAsync(cancellationToken);

    private IQueryable<Booking> ActiveBookings(Guid screeningId) =>
        Context.Bookings.Where(b =>
            b.ScreeningId == screeningId &&
            b.Status != BookingStatus.Cancelled);
}
