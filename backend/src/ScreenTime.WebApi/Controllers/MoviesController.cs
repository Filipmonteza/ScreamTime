using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Infrastructure.Identity;
using ScreenTime.WebApi.Filters;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[DomainExceptionFilter]
public class MoviesController : ControllerBase
{
    private const string ManageMoviesRoles = $"{IdentitySeeder.AdminRole},{IdentitySeeder.StaffRole}";

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
        => Ok(await _movieService.GetMovieByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = ManageMoviesRoles)]
    public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto dto)
    {
        var created = await _movieService.CreateMovieAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.MovieId }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageMoviesRoles)]
    public async Task<IActionResult> UpdateMovie(Guid id, UpdateMovieDto dto)
    {
        await _movieService.UpdateMovieAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageMoviesRoles)]
    public async Task<IActionResult> DeleteMovie(Guid id)
    {
        await _movieService.DeleteMovieAsync(id);
        return NoContent();
    }
}