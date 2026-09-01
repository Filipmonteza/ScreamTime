using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IGenreService
{
    Task<List<GenreDto>> GetAllGenresAsync();
}