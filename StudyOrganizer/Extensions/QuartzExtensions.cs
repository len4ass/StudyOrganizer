using Quartz;
using StudyOrganizer.Services.TriggerService;
using StudyOrganizer.Settings.SimpleTrigger;

namespace StudyOrganizer.Extensions;

public static class QuartzExtensions
{
    public static ITrigger BuildTrigger(SimpleTrigger simpleTrigger)
    {
        if (simpleTrigger.Settings.RecurringType == SimpleTriggerRecurringType.EveryMinute)
        {
            return TriggerBuilder.Create()
                .WithIdentity(simpleTrigger.Name, "worker_service")
                .WithSchedule(
                    CronScheduleBuilder
                        .CronSchedule($"{simpleTrigger.Settings.SecondUtc} * * ? * * *")
                        .WithMisfireHandlingInstructionDoNothing()
                        .InTimeZone(TimeZoneInfo.Utc))
                .Build();
        }

        if (simpleTrigger.Settings.RecurringType == SimpleTriggerRecurringType.Hourly)
        {
            return TriggerBuilder.Create()
                .WithIdentity(simpleTrigger.Name, "worker_service")
                .WithSchedule(
                    CronScheduleBuilder
                        .CronSchedule(
                            $"{simpleTrigger.Settings.SecondUtc} " +
                            $"{simpleTrigger.Settings.MinuteUtc} * ? * * *")
                        .WithMisfireHandlingInstructionDoNothing()
                        .InTimeZone(TimeZoneInfo.Utc))
                .Build();
        }

        if (simpleTrigger.Settings.RecurringType == SimpleTriggerRecurringType.Daily)
        {
            return TriggerBuilder.Create()
                .WithIdentity(simpleTrigger.Name, "worker_service")
                .WithSchedule(
                    CronScheduleBuilder
                        .CronSchedule(
                            $"{simpleTrigger.Settings.SecondUtc} " +
                            $"{simpleTrigger.Settings.MinuteUtc} " +
                            $"{simpleTrigger.Settings.HourUtc} ? * * *")
                        .WithMisfireHandlingInstructionDoNothing()
                        .InTimeZone(TimeZoneInfo.Utc))
                .Build();
        }

        return TriggerBuilder.Create()
            .WithIdentity(simpleTrigger.Name, "worker_service")
            .WithSchedule(
                CronScheduleBuilder
                    .CronSchedule(
                        $"{simpleTrigger.Settings.SecondUtc} " +
                        $"{simpleTrigger.Settings.MinuteUtc} " +
                        $"{simpleTrigger.Settings.HourUtc} " +
                        $"{simpleTrigger.Settings.DayOfWeek} * * *")
                    .WithMisfireHandlingInstructionDoNothing()
                    .InTimeZone(TimeZoneInfo.Utc))
            .Build();
    }
}