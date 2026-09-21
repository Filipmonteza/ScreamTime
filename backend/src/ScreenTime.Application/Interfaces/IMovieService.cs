using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto> GetMovieByIdAsync(Guid id);
    Task<MovieDto> CreateMovieAsync(CreateMovieDto dto);
    Task UpdateMovieAsync(Guid id, UpdateMovieDto dto);
    Task DeleteMovieAsync(Guid id);
}