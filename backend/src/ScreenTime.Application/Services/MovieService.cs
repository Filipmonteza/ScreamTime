using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return movies.Select(MapToDto).ToList();
    }

    public async Task<MovieDto?> GetMovieByIdAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        return movie is null ? null : MapToDto(movie);
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto dto)
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            AgeRating = dto.AgeRating,
            RuntimeMinutes = dto.RuntimeMinutes,
            Description = dto.Description,
            IsActive = true,
            MovieGenres = dto.GenreIds.Select(gid => new MovieGenre
            {
                GenreId = gid
            }).ToList()
        };

        await _movieRepository.AddAsync(movie);
        await _movieRepository.SaveChangesAsync();

        var created = await _movieRepository.GetByIdAsync(movie.Id);
        return MapToDto(movie);
    }

    public async Task<bool> UpdateMovieAsync(Guid id, UpdateMovieDto dto)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null) return false;

        var genres = await _movieRepository.GetGenresByIdsAsync(dto.GenreIds);

        movie.Title = dto.Title;
        movie.AgeRating = dto.AgeRating;
        movie.RuntimeMinutes = dto.RuntimeMinutes;
        movie.Description = dto.Description;
        movie.MovieGenres = genres.Select(g => new MovieGenre
        {
            MovieId = movie.Id,
            GenreId = g.Id
        }).ToList();

        _movieRepository.Update(movie);
        return await _movieRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteMovieAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null) return false;

        movie.IsActive = false; // soft delete
        _movieRepository.Update(movie);
        return await _movieRepository.SaveChangesAsync();
    }

    private static MovieDto MapToDto(Movie movie) => new(
        movie.Id,
        movie.Title,
        movie.AgeRating,
        movie.RuntimeMinutes,
        movie.Description,
        movie.IsActive,
        movie.Genres.Select(g => g.Name).ToList()
    );
}