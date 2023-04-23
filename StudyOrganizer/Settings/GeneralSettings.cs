using System.Text.Json.Serialization;
using StudyOrganizer.Converters;

namespace StudyOrganizer.Settings;

public sealed class GeneralSettings
{
    public long OwnerId { get; init; } = default!;
    public long MainChatId { get; init; } = default!;
    public long ImportantChatId { get; init; } = default!;

    [JsonConverter(typeof(TimeZoneInfoConverter))]
    public TimeZoneInfo ChatTimeZoneUtc { get; init; } = default!;
}