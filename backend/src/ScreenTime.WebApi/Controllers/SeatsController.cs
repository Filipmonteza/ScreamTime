using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatsController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    // The price list. Anonymous on purpose - the seat map shows what a Standard
    // and a VIP seat cost before anyone signs in, so there is nothing to guard.
    [HttpGet("types")]
    public async Task<ActionResult<List<SeatTypeDto>>> GetAllSeatTypes()
    => Ok(await _seatService.GetAllSeatTypesAsync());
}