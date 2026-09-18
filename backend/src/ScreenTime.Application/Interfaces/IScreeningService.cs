using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// The screening flow: what is playing, and - for staff - scheduling it.
/// </summary>
/// <remarks>
/// The write methods throw rather than returning a bare null or false: a screening
/// can be refused for half a dozen unrelated reasons - unknown film, deactivated
/// auditorium, a clash with another screening, a start time in the past, bookings
/// already sold - and a single return value cannot say which.
/// DomainExceptionFilterAttribute maps the exceptions onto status codes.
/// </remarks>
public interface IScreeningService
{
    Task<List<ScreeningDto>> GetScreeningsAsync(
        ScreeningFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<ScreeningDto> GetScreeningByIdAsync(
        Guid id,
        bool includeCancelled = false,
        CancellationToken cancellationToken = default);

    Task<ScreeningDto> CreateScreeningAsync(
        CreateScreeningDto dto,
        CancellationToken cancellationToken = default);

    Task<ScreeningDto> UpdateScreeningAsync(
        Guid id,
        UpdateScreeningDto dto,
        CancellationToken cancellationToken = default);

    // Cancelled, not deleted: the screening stays as a record, and the bookings
    // sold against it are cancelled with it.
    Task<ScreeningDto> CancelScreeningAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
