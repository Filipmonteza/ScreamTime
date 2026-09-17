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
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
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
            throw new ArgumentException(setEmailResult.Errors.First().Description);

        // UserName mirrors Email throughout the app (see AuthController.Register).
        var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.NewEmail);
        if (!setUserNameResult.Succeeded)
            throw new ArgumentException(setUserNameResult.Errors.First().Description);

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
            throw new ArgumentException(result.Errors.First().Description);

        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}