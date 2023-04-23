using Quartz;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJob
{
    public SimpleTrigger LoadedTrigger { get; set; } = default!;
    public JobKey JobKey { get; set; } = default!;
    public TriggerKey TriggerKey { get; set; } = default!;
}