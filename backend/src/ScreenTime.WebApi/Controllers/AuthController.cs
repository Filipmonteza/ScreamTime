using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Contracts.Authentication;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.WebApi.Controllers;

/// <summary>
/// Handles user authentication operations including login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials (email and password).</param>
    /// <returns>JWT token and user information if successful.</returns>
    /// <response code="200">Successfully authenticated and returned JWT token.</response>
    /// <response code="400">Invalid request format.</response>
    /// <response code="401">Invalid credentials or account locked.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User with email {Email} not found", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Check password and sign-in requirements
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login attempt failed: User {UserId} account is locked", user.Id);
                return Unauthorized(new { message = "Account is locked due to multiple failed login attempts." });
            }

            _logger.LogWarning("Login attempt failed: Invalid password for user {UserId}", user.Id);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Get user's primary role (first role if multiple exist)
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault();

        // Generate JWT token
        var tokenInfo = new UserTokenInfo
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty
        };

        var (token, expiration) = await _jwtTokenService.GenerateTokenAsync(tokenInfo, primaryRole);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Ok(new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = primaryRole
        });
    }

    /// <summary>
    /// Registers a new user, assigns the Customer role and returns a JWT token.
    /// Immediate sign in without requiring email confirmation.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            return BadRequest(new { errors = new[] { new { Code = "DuplicateEmail", Description = "A user with this email already exists." } } });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true // immediate sign in without email confirmation
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new { errors = createResult.Errors.Select(e => new { e.Code, e.Description }) });
        }

        // Admin/Staff/Customer are seeded at startup by IdentitySeeder.
        var addRoleResult = await _userManager.AddToRoleAsync(user, IdentitySeeder.CustomerRole);
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new { errors = addRoleResult.Errors.Select(e => new { e.Code, e.Description }) });
        }

        var tokenInfo = new UserTokenInfo
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty
        };

        var (token, expiration) = await _jwtTokenService.GenerateTokenAsync(tokenInfo, IdentitySeeder.CustomerRole);

        _logger.LogInformation("New user {UserId} registered successfully", user.Id);

        var response = new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = IdentitySeeder.CustomerRole
        };

        return Created(string.Empty, response);
    }
}
