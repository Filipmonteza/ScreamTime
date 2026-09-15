using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ScreenTime.Application.Contracts;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.WebApi.Controllers;

/// <summary>
/// Admin-only user administration: list accounts and assign roles.
/// Note: GetUsers() returns only users in the Admin or Staff roles (i.e. staff accounts), not all users.
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
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (roles.Contains(IdentitySeeder.AdminRole) || roles.Contains(IdentitySeeder.StaffRole))
            {
                var isActive = !(user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow);
                results.Add(new AdminUserDto(user.Id, user.Email ?? string.Empty, roles, isActive));
            }
        }

        return Ok(results);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { error = "Email and password are required." });

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null) return BadRequest(new { error = "A user with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded) return BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });

        var roles = dto.Roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct().ToList() ?? new List<string>();
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new { error = $"Role '{role}' does not exist." });
            }
        }

        if (roles.Any())
        {
            var addRoles = await _userManager.AddToRolesAsync(user, roles);
            if (!addRoles.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new { errors = addRoles.Errors.Select(e => e.Description) });
            }
        }

        var createdDto = new AdminUserDto(user.Id, user.Email ?? string.Empty, roles, true);
        return CreatedAtAction(nameof(GetUsers), null, createdDto);
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.NewEmail) && dto.NewEmail != user.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, dto.NewEmail);
            if (!setEmailResult.Succeeded) return BadRequest(new { errors = setEmailResult.Errors.Select(e => e.Description) });

            var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.NewEmail);
            if (!setUserNameResult.Succeeded) return BadRequest(new { errors = setUserNameResult.Errors.Select(e => e.Description) });
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetResult.Succeeded) return BadRequest(new { errors = resetResult.Errors.Select(e => e.Description) });
        }

        return NoContent();
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        // Prevent admins from deleting their own account
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentUserId == user.Id)
        {
            return BadRequest(new { error = "Admins cannot delete their own account." });
        }

        // Deactivate the user by locking them out indefinitely instead of deleting
        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!lockoutResult.Succeeded) return BadRequest(new { errors = lockoutResult.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpPost("users/{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> UpdateUserRoles(Guid id, [FromBody] UpdateUserRolesDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        // Prevent admins from changing their own roles
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentUserId == user.Id)
        {
            return BadRequest(new { error = "Admins cannot change their own roles." });
        }

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
