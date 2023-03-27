using System.Text.Json.Serialization;

namespace StudyOrganizer.Settings;

public sealed class GeneralSettings
{
    public long OwnerId { get; init; }
    public long MainChatId { get; init; }
    public long ImportantChatId { get; init; }
    public string ChatTimeZoneUtc { get; init; }
}