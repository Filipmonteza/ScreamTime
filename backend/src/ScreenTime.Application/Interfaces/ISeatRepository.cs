using ScreenTime.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Interfaces;

public interface ISeatRepository
{
    Task<List<Seat>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<Seat>> GetByAuditoriumIdAsync(
        Guid auditoriumId,
        CancellationToken cancellationToken = default);

    Task<List<SeatType>> GetSeatTypesByIdsAsync(
        List<Guid> seatTypeIds,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Seat> seats,
        CancellationToken cancellationToken = default);

    Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<List<SeatType>> GetAllSeatTypesAsync(
        CancellationToken cancellationToken = default);
}