using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

/// <summary>
/// Request DTO for user login.
/// </summary>
public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;
}
