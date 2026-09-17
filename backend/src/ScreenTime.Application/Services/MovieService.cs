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

    public async Task<MovieDto> GetMovieByIdAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Filmen hittades inte.");

        return MapToDto(movie);
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto dto)
    {
        // Remove duplicate genre IDs
        var genreIds = dto.GenreIds.Distinct().ToList();

        // Get the genres from the database
        var genres = await _movieRepository.GetGenresByIdsAsync(genreIds);

        // Make sure all requested genres exist
        if (genres.Count != genreIds.Count)
            throw new ArgumentException("En eller flera GenreIds finns inte.");

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            AgeRating = dto.AgeRating,
            RuntimeMinutes = dto.RuntimeMinutes,
            Description = dto.Description,
            IsActive = true,
            Genres = genres
        };

        await _movieRepository.AddAsync(movie);
        await _movieRepository.SaveChangesAsync();

        return MapToDto(movie);
    }

    public async Task UpdateMovieAsync(Guid id, UpdateMovieDto dto)
    {
        var movie = await _movieRepository.GetByIdAsync(id, includeInactive: true, track: true);

        if (movie is null)
            throw new KeyNotFoundException("Filmen hittades inte.");

        // Remove duplicate genre IDs
        var genreIds = dto.GenreIds.Distinct().ToList();

        // Get the genres from the database
        var genres = await _movieRepository.GetGenresByIdsAsync(genreIds);

        // Make sure all requested genres exist
        if (genres.Count != genreIds.Count)
            throw new ArgumentException("En eller flera GenreIds finns inte.");

        movie.Title = dto.Title;
        movie.AgeRating = dto.AgeRating;
        movie.RuntimeMinutes = dto.RuntimeMinutes;
        movie.Description = dto.Description;
        movie.IsActive = dto.IsActive;
        movie.Genres = genres;

        _movieRepository.Update(movie);

        await _movieRepository.SaveChangesAsync();
    }

    public async Task DeleteMovieAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id, track: true);

        if (movie is null)
            throw new KeyNotFoundException("Filmen hittades inte.");

        movie.IsActive = false; // Soft delete

        _movieRepository.Update(movie);

        await _movieRepository.SaveChangesAsync();
    }

    private static MovieDto MapToDto(Movie movie) => new(
        movie.Id,
        movie.Title,
        movie.AgeRating,
        movie.RuntimeMinutes,
        movie.Description,
        movie.IsActive,
        movie.Genres.Select(g => g.Name).ToList(),
        movie.Poster
    );
}