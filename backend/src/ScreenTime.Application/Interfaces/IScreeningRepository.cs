using ScreenTime.Application.Contracts;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

// Screenings are cancelled rather than deleted, so the inherited reads leave
// cancelled ones out.
public interface IScreeningRepository : IRepository<Screening>
{
    // Staff views that need to open a cancelled screening use this overload.
    // Pass includeCancelled by name at the call site - it is deliberately
    // required, which is what keeps it from colliding with the inherited
    // GetByIdAsync.
    Task<Screening?> GetByIdAsync(
        Guid id,
        bool includeCancelled,
        bool track = false,
        CancellationToken cancellationToken = default);

    // The programme, narrowed by whatever the filter carries and ordered by
    // start time.
    Task<List<Screening>> GetFilteredAsync(
        ScreeningFilterDto filter,
        CancellationToken cancellationToken = default);

    // True when a live screening already occupies part of [startTime, endTime)
    // in this auditorium. excludeScreeningId keeps an update from clashing with
    // the screening it is updating.
    Task<bool> HasOverlapAsync(
        Guid auditoriumId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeScreeningId = null,
        CancellationToken cancellationToken = default);

    // True when the screening still holds bookings that nobody has cancelled.
    Task<bool> HasActiveBookingsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default);

    // Those same bookings, tracked and with their tickets, so cancelling the
    // screening can cancel them and release the seats.
    Task<List<Booking>> GetActiveBookingsAsync(
        Guid screeningId,
        CancellationToken cancellationToken = default);
}
