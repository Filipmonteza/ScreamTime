using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumRepository
{
    Task<List<Auditorium>> GetAllAsync(
        bool includeInactive = false);
    Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive = false,
        bool track = false);
    Task<List<Seat>> GetSeatsByAuditoriumIdAsync(
        Guid auditoriumId);
    Task<int> GetCapacityAsync(Guid auditoriumId);
    Task<bool> AuditoriumNameExistsAsync(string name);
    Task AddAsync(Auditorium auditorium);
    Task<bool> SaveChangesAsync();
}

