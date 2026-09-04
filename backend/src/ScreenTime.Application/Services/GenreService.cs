using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.Application.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<List<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        return genres.Select(g => new GenreDto(g.Id, g.Name)).ToList();
    }
}