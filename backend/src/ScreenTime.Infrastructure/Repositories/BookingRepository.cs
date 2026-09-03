using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Exceptions;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure.Repositories;

// EF Core implementation of IBookingRepository.
public class BookingRepository : IBookingRepository
{
    private readonly ScreenTimeDbContext _context;

    public BookingRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    // Tracked: CancelBookingAsync mutates the booking and releases its tickets.
    public async Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default) =>
        await BookingsWithDetails()
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await BookingsWithDetails()
            .AsNoTracking()
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

    public async Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default) =>
        await _context.Bookings.AddAsync(booking, cancellationToken);

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
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
        await _context.Screenings
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
        await _context.Bookings.AnyAsync(
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

    // The graph BookingService reads off a Booking.
    private IQueryable<Booking> BookingsWithDetails() =>
        _context.Bookings
            .Include(b => b.Screening).ThenInclude(s => s.Movie)
            .Include(b => b.Screening).ThenInclude(s => s.Auditorium)
            .Include(b => b.Tickets).ThenInclude(t => t.Seat)
                .ThenInclude(s => s.SeatType);

    // SeatType carries the price, which every seat DTO needs.
    private IQueryable<Seat> SeatsWithType() =>
        _context.Seats
            .Include(s => s.SeatType)
            .AsNoTracking();

    // A released ticket is history, not an occupied seat.
    private IQueryable<Ticket> ActiveTickets() =>
        _context.Tickets.Where(t => t.ReleasedAt == null);

    // SQLite reports a violated unique index as extended result code 2067. The
    // only unique index on Tickets is the one-active-ticket-per-seat guard, so
    // a 2067 naming that table is always a seat collision.
    private static bool IsSeatAlreadyTaken(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 } inner
        && inner.Message.Contains("Tickets", StringComparison.OrdinalIgnoreCase);
}
