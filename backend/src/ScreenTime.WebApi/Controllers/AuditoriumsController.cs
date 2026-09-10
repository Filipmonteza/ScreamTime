using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriumsController : ControllerBase
{
    private const string ManageAuditoriumsRoles = $"{IdentitySeeder.AdminRole},{IdentitySeeder.StaffRole}";

    private readonly IAuditoriumService _auditoriumService;
    private readonly ISeatService _seatService;

    public AuditoriumsController(
        IAuditoriumService auditoriumService,
        ISeatService seatService)
    {
        _auditoriumService = auditoriumService;
        _seatService = seatService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditoriumListDto>>> GetAll(bool includeInactive = false)
    {
        if (includeInactive &&
            !User.IsInRole(IdentitySeeder.AdminRole) &&
            !User.IsInRole(IdentitySeeder.StaffRole))
        {
            return Forbid();
        }

        return Ok(await _auditoriumService.GetAllAuditoriumsAsync(includeInactive));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditoriumDetailsDto>> GetById(
        Guid id,
        bool includeInactive = false)
    {
        if (includeInactive &&
            !User.IsInRole(IdentitySeeder.AdminRole) &&
            !User.IsInRole(IdentitySeeder.StaffRole))
        {
            return Forbid();
        }

        var auditorium = await _auditoriumService.GetAuditoriumByIdAsync(id, includeInactive);

        return auditorium is null ? NotFound() : Ok(auditorium);
    }

    [HttpPost]
    [Authorize(Roles = ManageAuditoriumsRoles)]
    public async Task<ActionResult<AuditoriumDto>> CreateAuditorium(AuditoriumCreateDto dto)
    {
        var created = await _auditoriumService.CreateAuditoriumAsync(dto);

        if (created is null)
            return BadRequest("Ogiltigt eller redan använt salongsnamn.");

        return CreatedAtAction(nameof(GetById), new { id = created.AuditoriumId }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageAuditoriumsRoles)]
    public async Task<IActionResult> UpdateAuditorium(
        Guid id,
        AuditoriumUpdateDto dto)
    {
        var result = await _auditoriumService.UpdateAuditoriumAsync(id, dto);

        if (result is null)
            return BadRequest("Ogiltigt eller redan använt salongsnamn.");

        return result.Value ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageAuditoriumsRoles)]
    public async Task<IActionResult> DeleteAuditorium(Guid id)
        => await _auditoriumService.DeleteAuditoriumAsync(id)
            ? NoContent()
            : NotFound();

    [HttpPost("{id:guid}/seats")]
    [Authorize(Roles = ManageAuditoriumsRoles)]
    public async Task<IActionResult> AddSeats(Guid id, AddSeatsRequestDto dto)
    {
        var request = new AddSeatsToAuditoriumDto(id, dto.Seats);
        var result = await _seatService.AddSeatsToAuditoriumAsync(request);

        return result switch
        {
            null => BadRequest("Ogiltig sätesdata, dubbletter, eller okänd sätestyp."),
            false => NotFound("Salongen hittades inte."),
            true => NoContent()
        };
    }
}