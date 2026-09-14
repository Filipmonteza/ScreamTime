using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface ISeatRepository : IRepository<Seat>
{
    Task<List<Seat>> GetByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default);

    // SeatType is a separate entity, so these two do not come from the base.
    Task<List<SeatType>> GetSeatTypesByIdsAsync(
        List<Guid> seatTypeIds,
        CancellationToken cancellationToken = default);

    Task<List<SeatType>> GetAllSeatTypesAsync(
        CancellationToken cancellationToken = default);
}
