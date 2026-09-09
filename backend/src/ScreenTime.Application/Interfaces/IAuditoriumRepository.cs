using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumRepository
{
    Task<bool> AuditoriumNameExistsAsync(string name);
    Task AddAsync(Auditorium auditorium);
    Task<bool> SaveChangesAsync();
}


