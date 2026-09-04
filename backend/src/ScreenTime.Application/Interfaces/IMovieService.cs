using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto?> GetMovieByIdAsync(Guid id);
    Task<MovieDto?> CreateMovieAsync(CreateMovieDto dto);
    Task<bool?> UpdateMovieAsync(Guid id, UpdateMovieDto dto);
    Task<bool> DeleteMovieAsync(Guid id);
}