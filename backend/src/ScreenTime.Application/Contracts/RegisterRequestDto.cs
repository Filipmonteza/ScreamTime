using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

/// <summary>
/// Request DTO for user registration.
/// </summary>
public record RegisterRequestDto
{
    [Required(ErrorMessage = "E-post krävs.")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lösenord krävs.")]
    [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Du måste upprepa lösenordet.")]
    [Compare(nameof(Password), ErrorMessage = "Lösenorden måste matcha.")]
    public string ConfirmPassword { get; init; } = string.Empty;

}
