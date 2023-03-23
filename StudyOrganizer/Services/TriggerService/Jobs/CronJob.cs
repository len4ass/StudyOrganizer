using Serilog;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJob : IJob
{
    private readonly SimpleTrigger _simpleTrigger;

    public CronJob(SimpleTrigger simpleTrigger)
    {
        _simpleTrigger = simpleTrigger;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            Log.Logger.Information($"Таска {_simpleTrigger.Name} запущена.");
            while (!cancellationToken.IsCancellationRequested)
            {
                var nextTask = new DateTimeOffset(
                    DateTimeOffset.UtcNow.Year,
                    DateTimeOffset.UtcNow.Month,
                    DateTimeOffset.UtcNow.Day,
                    _simpleTrigger.Settings.HourUtc,
                    _simpleTrigger.Settings.MinuteUtc,
                    _simpleTrigger.Settings.SecondUtc,
                    TimeSpan.Zero);
                
                while (nextTask - DateTimeOffset.UtcNow < TimeSpan.Zero)
                {
                    nextTask += new TimeSpan(0, 0, 0, _simpleTrigger.Settings.RunEveryGivenSeconds);
                }

                var timeLeftTillNextTask = nextTask - DateTimeOffset.UtcNow;
                try
                {
                    await Task.Delay(timeLeftTillNextTask, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (_simpleTrigger.Settings.ShouldRun)
                {
                    await _simpleTrigger.ExecuteAsync();
                    Log.Logger.Information($"Таска {_simpleTrigger.Name} успешно отработала.");
                }
            }
            
            Log.Logger.Information($"Завершение таски {_simpleTrigger.Name}...");
        }, cancellationToken);
    }

    public void Enable()
    {
        _simpleTrigger.Settings.ShouldRun = true;
    }

    public void Disable()
    {
        _simpleTrigger.Settings.ShouldRun = false;
    }

    public SimpleTrigger GetInternalTrigger()
    {
        return _simpleTrigger;
    }
}