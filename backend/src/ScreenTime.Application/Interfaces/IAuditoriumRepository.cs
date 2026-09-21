using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumRepository : IRepository<Auditorium>
{
    // Auditoriums are soft-deleted, so the inherited reads return active auditoriums
    // only. Admin views that list or open a deactivated auditorium use these two.
    // includeInactive is deliberately required rather than optional.
    Task<List<Auditorium>> GetAllAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive,
        bool track = false,
        CancellationToken cancellationToken = default);

    // Seat counts per auditorium in one grouped query.
    Task<Dictionary<Guid, int>> GetCapacitiesAsync(
        IEnumerable<Guid> auditoriumIds,
        CancellationToken cancellationToken = default);

    Task<bool> AuditoriumNameExistsAsync(
        string name,
        Guid? excludeAuditoriumId = null,
        CancellationToken cancellationToken = default);
}
