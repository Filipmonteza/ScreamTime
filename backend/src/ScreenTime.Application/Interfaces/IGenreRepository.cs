using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync();
}