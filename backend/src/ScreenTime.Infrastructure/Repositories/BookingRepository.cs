using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Exceptions;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

// EF Core implementation of IBookingRepository.
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ScreenTimeDbContext context) : base(context)
    {
    }

    // The graph BookingService reads off a Booking. Tracked only when the caller
    // asked for it - CancelBookingAsync mutates the booking and its tickets.
    protected override IQueryable<Booking> Query(bool track = false)
    {
        var bookings = Entities
            .Include(b => b.Screening).ThenInclude(s => s.Movie)
            .Include(b => b.Screening).ThenInclude(s => s.Auditorium)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
                .ThenInclude(s => s.SeatType);

        return track ? bookings : bookings.AsNoTracking();
    }

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(b => b.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<bool> AreSeatsAvailableAsync(
        Guid screeningId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        var ids = seatIds.ToList();

        var anyOccupied = await ActiveTickets()
            .AnyAsync(
                t => t.ScreeningId == screeningId && ids.Contains(t.SeatId),
                cancellationToken);

        return !anyOccupied;
    }

    public override async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSeatAlreadyTaken(exception))
        {
            // Someone took the seat between the availability check and this
            // insert. The unique index caught it; turn it into the conflict the
            // service layer already knows how to describe.
            throw new SeatsUnavailableException();
        }
    }

    public async Task<Screening?> GetScreeningAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default) =>
        await Context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken);

    public async Task<IReadOnlyList<Seat>> GetSeatsByIdsAsync(
        Guid auditoriumId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        var ids = seatIds.ToList();

        return await SeatsWithType()
            .Where(s => s.AuditoriumId == auditoriumId && ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> BookingReferenceExistsAsync(
        string bookingReference,
        CancellationToken cancellationToken = default) =>
        await Entities.AnyAsync(
            b => b.BookingReference == bookingReference,
            cancellationToken);

    public async Task<IReadOnlyList<Seat>> GetSeatsByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default) =>
        await SeatsWithType()
            .Where(s => s.AuditoriumId == auditoriumId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetUnavailableSeatIdsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default) =>
        await ActiveTickets()
            .Where(t => t.ScreeningId == screeningId)
            .Select(t => t.SeatId)
            .ToListAsync(cancellationToken);

    // SeatType carries the price, which every seat DTO needs.
    private IQueryable<Seat> SeatsWithType() =>
        Context.Seats
            .Include(s => s.SeatType)
            .AsNoTracking();

    // A released ticket is history, not an occupied seat.
    private IQueryable<Ticket> ActiveTickets() =>
        Context.Tickets.Where(t => t.ReleasedAt == null);

    // SQLite reports a violated unique index as extended result code 2067. The
    // only unique index on Tickets is the one-active-ticket-per-seat guard, so
    // a 2067 naming that table is always a seat collision.
    private static bool IsSeatAlreadyTaken(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 } inner
        && inner.Message.Contains("Tickets", StringComparison.OrdinalIgnoreCase);
}
