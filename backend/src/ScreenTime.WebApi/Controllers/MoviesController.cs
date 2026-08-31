using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    //Create Movie Endpoint
    [HttpPost]
    public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto dto)
    {
        // Implementation will be added here.

        return Ok();
    }
}