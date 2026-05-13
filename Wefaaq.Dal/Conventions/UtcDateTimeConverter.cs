using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Wefaaq.Dal.Conventions;

// SQL Server's datetime2 doesn't store DateTimeKind, so EF reads values back with Kind=Unspecified.
// That makes System.Text.Json drop the trailing 'Z', and downstream consumers (including the browser)
// silently treat the string as local time. Tagging the value as UTC on read fixes it for every consumer.
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
                : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
