namespace StudyOrganizer.Settings;

public sealed class TriggerSettings
{
    public bool ShouldRun { get; init; } 
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public int RunEveryGivenSeconds { get; init; }
}