using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetAll()
        => Ok(await _movieService.GetAllMoviesAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovieDto>> GetById(Guid id)
    {
        var movie = await _movieService.GetMovieByIdAsync(id);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost]
    public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto dto)
    {
        var created = await _movieService.CreateMovieAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.MovieId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMovie(Guid id, UpdateMovieDto dto)
    {
        var result = await _movieService.UpdateMovieAsync(id, dto);
        if (result is null) return BadRequest("En eller flera GenreIds finns inte.");
        return result.Value ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMovie(Guid id)
        => await _movieService.DeleteMovieAsync(id) ? NoContent() : NotFound();
}