namespace ScreenTime.Domain.Common;

/// <summary>
/// Every DateTime the application stores, compares or sends is UTC. The cinema
/// itself runs on Swedish time - that is what "the 18:00 show" and "screenings
/// on the 20th" mean - and this is the place they are translated.
/// </summary>
public static class CinemaClock
{
    public static readonly TimeZoneInfo TimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    /// <summary>
    /// The instant a value names, as UTC. A local value is converted; a value
    /// with no kind at all is taken to already be UTC.
    /// </summary>
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

    /// <summary>
    /// A wall-clock time in the cinema's own zone, as the UTC instant it names.
    /// </summary>
    public static DateTime FromCinemaTime(DateTime wallClock) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified),
            TimeZone);

    /// <summary>
    /// Today's date on the cinema's calendar, wherever the server happens to be.
    /// </summary>
    public static DateOnly Today =>
        DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone));
}
