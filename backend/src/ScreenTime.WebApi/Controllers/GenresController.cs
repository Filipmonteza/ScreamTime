using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    // The catalogue's genre vocabulary. Anonymous on purpose - the same names are
    // printed on every film card, and the admin film form reads the ids from here.
    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> GetAll()
        => Ok(await _genreService.GetAllGenresAsync());
}