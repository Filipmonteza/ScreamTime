using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

public record AccountDto(Guid UserId, string Email);

public record UpdateEmailDto(
    [Required, EmailAddress] string NewEmail,
    [Required] string CurrentPassword
);

public record UpdatePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);