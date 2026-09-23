using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Contracts.Authentication;
using ScreenTime.Infrastructure.Identity;
using ScreenTime.WebApi.Filters;
using System.Security.Claims;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DomainExceptionFilter]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpGet]
    public async Task<ActionResult<AccountDto>> GetAccount()
    {
        var user = await _userManager.FindByIdAsync(GetUserId().ToString());
        if (user is null) throw new KeyNotFoundException("Användaren hittades inte.");

        return Ok(new AccountDto(user.Id, user.Email ?? string.Empty));
    }

    [HttpPut("email")]
    public async Task<ActionResult<AuthResponse>> UpdateEmail(UpdateEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(GetUserId().ToString());
        if (user is null) throw new KeyNotFoundException("Användaren hittades inte.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!passwordValid)
            throw new ArgumentException("Fel lösenord.");

        var setEmailResult = await _userManager.SetEmailAsync(user, dto.NewEmail);
        if (!setEmailResult.Succeeded)
            throw new ArgumentException(string.Join(" ", setEmailResult.Errors.Select(e => e.Description)));

        // UserName mirrors Email throughout the app (see AuthController.Register).
        var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.NewEmail);
        if (!setUserNameResult.Succeeded)
            throw new ArgumentException(string.Join(" ", setUserNameResult.Errors.Select(e => e.Description)));

        // Reissue the token: it carries the old email as a claim.
        var roles = await _userManager.GetRolesAsync(user);
        var tokenInfo = new UserTokenInfo { Id = user.Id, Email = user.Email ?? string.Empty };
        var (token, expiration) = await _jwtTokenService.GenerateTokenAsync(tokenInfo, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault()
        });
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(GetUserId().ToString());
        if (user is null) throw new KeyNotFoundException("Användaren hittades inte.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(e => e.Description)));

        return NoContent();
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        var user = await _userManager.FindByIdAsync(GetUserId().ToString());
        if (user is null) return NotFound();

        // Only an admin can reactivate an account, so an admin closing their own
        // could leave nobody able to. AdminController refuses the same thing.
        if (await _userManager.IsInRoleAsync(user, IdentitySeeder.AdminRole))
            throw new InvalidOperationException("En administratör kan inte avaktivera sitt eget konto.");

        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!lockoutResult.Succeeded)
            return BadRequest(new { errors = lockoutResult.Errors.Select(e => new { e.Code, e.Description }) });

        // Sign out server-side (covers cookie scenarios). Clients should also remove stored tokens.
        try
        {
            await _signInManager.SignOutAsync();
        }
        catch (InvalidOperationException)
        {
            // No sign-out handlers are registered (JWT-only configuration). Safe to ignore.
        }

        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}