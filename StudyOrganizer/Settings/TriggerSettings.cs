using Microsoft.EntityFrameworkCore;

namespace StudyOrganizer.Settings;

[Owned]
public sealed class TriggerSettings
{
    public bool ShouldRun { get; set; } 
    public int HourUtc { get; init; }
    public int MinuteUtc { get; init; }
    public int SecondUtc { get; init; }
    public int RunEveryGivenSeconds { get; init; }
}