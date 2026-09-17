using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScreenTime.Domain.Common;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Normalises anything that is not already UTC.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => CinemaClock.ToUtc(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
