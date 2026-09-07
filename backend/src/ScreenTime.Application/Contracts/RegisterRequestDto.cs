using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

/// <summary>
/// Request DTO for user registration.
/// </summary>
public record RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;

    // Optional profile fields
    public string? FirstName { get; init; }
    public string? LastName { get; init; }

    // Optional birthdate
    public DateTime? Birthdate { get; init; }
}
