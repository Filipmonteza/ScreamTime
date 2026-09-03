namespace ScreenTime.Application.Contracts;

/// <summary>
/// Response DTO returned after successful registration (mirrors AuthResponse).
/// </summary>
public record RegisterResponseDto
{
    public string Token { get; init; } = string.Empty;

    public DateTime Expiration { get; init; }

    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? Role { get; init; }
}
