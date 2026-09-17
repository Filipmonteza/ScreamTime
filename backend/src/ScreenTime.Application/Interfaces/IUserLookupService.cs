namespace ScreenTime.Application.Interfaces;

// A narrow window into Identity for the Application layer, which cannot
// reference ApplicationUser directly since that type lives in Infrastructure.
// Booking search needs to go from an email to a UserId (and back, for
// display), without pulling Identity into a layer that shouldn't know about it.
public interface IUserLookupService
{
    // Exact email match, mirroring UserManager.FindByEmailAsync - partial/
    // contains search on email is not supported this way.
    Task<Guid?> FindUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    // Batched, so displaying a page of bookings costs one query for all the
    // emails on it rather than one query per booking.
    Task<Dictionary<Guid, string>> GetEmailsByUserIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);
}