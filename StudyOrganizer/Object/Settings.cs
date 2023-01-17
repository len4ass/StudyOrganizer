namespace StudyOrganizer.Object;

public sealed class Settings
{
    public long OwnerId { get; init; }
    public long MainChatId { get; init; }
    public long ImportantChatId { get; init; }
    public string? Token { get; init; }
    public TimeZoneInfo? HostingTimeZoneUtc { get; init; }
    public TimeZoneInfo? ChatTimeZoneUtc { get; init; }
}