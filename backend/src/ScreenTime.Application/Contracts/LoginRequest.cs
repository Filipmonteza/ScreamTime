using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

/// <summary>
/// Request DTO for user login.
/// </summary>
public record LoginRequest
{
    [Required(ErrorMessage = "E-post krävs.")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lösenord krävs.")]
    [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken.")]
    public string Password { get; init; } = string.Empty;
}
