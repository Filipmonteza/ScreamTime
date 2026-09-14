using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Contracts;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.WebApi.Controllers;

/// <summary>
/// Admin-only user administration: list accounts and assign roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = IdentitySeeder.AdminRole)]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers()
    {
        var userEntities = await _userManager.Users.ToListAsync();
        var results = new List<AdminUserDto>();

        foreach (var user in userEntities)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(new AdminUserDto(user.Id, user.Email ?? string.Empty, roles));
        }

        return Ok(results);
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> UpdateUserRoles(Guid id, [FromBody] UpdateUserRolesDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        // Validate roles
        var desired = dto.Roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct().ToList() ?? new List<string>();
        foreach (var role in desired)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return BadRequest(new { error = $"Role '{role}' does not exist." });
            }
        }

        var current = (await _userManager.GetRolesAsync(user)).ToList();

        var toAdd = desired.Except(current).ToList();
        var toRemove = current.Except(desired).ToList();

        if (toAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded) return BadRequest(new { errors = addResult.Errors.Select(e => e.Description) });
        }

        if (toRemove.Any())
        {
            var remResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!remResult.Succeeded) return BadRequest(new { errors = remResult.Errors.Select(e => e.Description) });
        }

        return NoContent();
    }
}
