using Microsoft.EntityFrameworkCore;

namespace StudyOrganizer.Settings;

[Owned]
public sealed class TriggerSettings
{
    public bool ShouldRun { get; set; } 
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public int RunEveryGivenSeconds { get; init; }
}