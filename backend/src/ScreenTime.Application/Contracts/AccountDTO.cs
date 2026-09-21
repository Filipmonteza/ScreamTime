using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

public record AccountDto(Guid UserId, string Email);

public record UpdateEmailDto(
    [Required(ErrorMessage = "E-postadress krävs."), EmailAddress(ErrorMessage = "E-postadressen är ogiltig.")] string NewEmail,
    [Required(ErrorMessage = "Lösenord krävs.")] string CurrentPassword
);

public record UpdatePasswordDto(
    [Required(ErrorMessage = "Lösenord krävs.")] string CurrentPassword,
    [Required(ErrorMessage = "Nytt lösenord krävs."), MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken långt.")] string NewPassword
);