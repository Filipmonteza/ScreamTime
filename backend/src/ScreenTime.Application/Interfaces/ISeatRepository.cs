using ScreenTime.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Interfaces;

public interface ISeatRepository
{
    Task<List<Seat>> GetAllAsync();
    Task<List<Seat>> GetByAuditoriumIdAsync(
        Guid auditoriumId);
    Task<List<SeatType>> GetSeatTypesByIdsAsync(
        List<Guid> seatTypeIds);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    Task<bool> SaveChangesAsync();
}