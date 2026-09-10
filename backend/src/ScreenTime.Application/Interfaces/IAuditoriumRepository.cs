using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumRepository
{
    Task<List<Auditorium>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive = false,
        bool track = false,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetCapacitiesAsync(
        IEnumerable<Guid> auditoriumIds,
        CancellationToken cancellationToken = default);

    Task<bool> AuditoriumNameExistsAsync(
        string name,
        Guid? excludeAuditoriumId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Auditorium auditorium,
        CancellationToken cancellationToken = default);

    void Update(Auditorium auditorium);

    Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
