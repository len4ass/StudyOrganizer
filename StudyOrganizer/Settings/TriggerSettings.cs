using Microsoft.EntityFrameworkCore;
using StudyOrganizer.Services.TriggerService;

namespace StudyOrganizer.Settings;

[Owned]
public sealed class TriggerSettings
{
    public bool ShouldRun { get; set; }
    public SimpleTriggerRecurringType RecurringType { get; set; }
    public string DayOfWeek { get; set; }
    public int HourUtc { get; set; }
    public int MinuteUtc { get; set; }
    public int SecondUtc { get; set; }
}