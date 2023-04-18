using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Loaders;

namespace StudyOrganizer.Settings.SimpleTrigger;

[Owned]
public sealed class TriggerSettings
{
    public bool ShouldRun { get; set; }
    public SimpleTriggerRecurringType RecurringType { get; set; }
    public SimpleTriggerDayOfWeek DayOfWeek { get; set; }
    public int HourUtc { get; set; }
    public int MinuteUtc { get; set; }
    public int SecondUtc { get; set; }

    public static TriggerSettings GetTriggerSettingsFromKeyValuePairs(IDictionary<string, string> keyValuePairs)
    {
        var triggerSettings = new TriggerSettings();
        ReflectionHelper.ParseAndMapKeyValuePairsOnObject(triggerSettings, keyValuePairs);
        return triggerSettings;
    }
}