namespace ScreenTime.Application.Contracts;

// A generic page of results plus the count needed to know how many pages
// exist. Any resource that grows without bound (Bookings today, potentially
// others later) returns this instead of a bare List<T>, so pagination
// happens in the database query rather than after loading everything.
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}