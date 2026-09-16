namespace ScreenTime.Application.Contracts;

public record AdminUserDto(Guid Id, string Email, IEnumerable<string> Roles, bool IsActive);

/// <summary>
/// The complete set of roles the user should hold afterwards; roles not listed are removed.
/// </summary>
public record UpdateUserRolesDto(IEnumerable<string> Roles);

public record CreateUserDto(string Email, string Password, IEnumerable<string> Roles);

public record UpdateUserDto(string? NewEmail, string? NewPassword);
