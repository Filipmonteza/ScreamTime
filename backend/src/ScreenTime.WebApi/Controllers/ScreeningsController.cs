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
public class ScreeningsController : ControllerBase
{
    private const string ManageScreeningsRoles =
        $"{IdentitySeeder.AdminRole},{IdentitySeeder.StaffRole}";

    private readonly IScreeningService _screeningService;

    public ScreeningsController(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }

    // The programme. Anonymous on purpose - what is on this week is the thing
    // that brings customers in. The filters are spelled out as query parameters
    // rather than bound as one object so they show up individually in Swagger:
    // GET /api/screenings?genreId=...&date=2026-09-20&auditoriumId=...
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ScreeningDto>>> GetAll(
        CancellationToken cancellationToken,
        Guid? movieId = null,
        Guid? auditoriumId = null,
        Guid? genreId = null,
        DateOnly? date = null,
        bool includeCancelled = false,
        bool includePast = false)
    {
        // Cancelled and finished screenings are staff business.
        if ((includeCancelled || includePast) && !CanManageScreenings)
        {
            return Forbid();
        }

        var filter = new ScreeningFilterDto(
            movieId,
            auditoriumId,
            genreId,
            date,
            includeCancelled,
            includePast);

        return Ok(await _screeningService.GetScreeningsAsync(filter, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ScreeningDto>> GetById(
        Guid id,
        CancellationToken cancellationToken,
        bool includeCancelled = false)
    {
        if (includeCancelled && !CanManageScreenings)
        {
            return Forbid();
        }

        return Ok(await _screeningService.GetScreeningByIdAsync(
            id,
            includeCancelled,
            cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = ManageScreeningsRoles)]
    public async Task<ActionResult<ScreeningDto>> CreateScreening(
        CreateScreeningDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _screeningService.CreateScreeningAsync(
            dto,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.ScreeningId },
            created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageScreeningsRoles)]
    public async Task<ActionResult<ScreeningDto>> UpdateScreening(
        Guid id,
        UpdateScreeningDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _screeningService.UpdateScreeningAsync(id, dto, cancellationToken));

    // POST rather than DELETE, the same way a booking is cancelled: the
    // screening survives as a cancelled record and the caller gets it back.
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = ManageScreeningsRoles)]
    public async Task<ActionResult<ScreeningDto>> CancelScreening(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await _screeningService.CancelScreeningAsync(id, cancellationToken));

    private bool CanManageScreenings =>
        User.IsInRole(IdentitySeeder.AdminRole) ||
        User.IsInRole(IdentitySeeder.StaffRole);
}
