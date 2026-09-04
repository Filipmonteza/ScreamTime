using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController : ControllerBase
{
    private readonly IScreeningService _screeningService;

    public ScreeningsController(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ScreeningDto>>> GetAll()
        => Ok(await _screeningService.GetAllScreeningsAsync());
}