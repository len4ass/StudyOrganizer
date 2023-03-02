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
                var nextTask = new DateTime(
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _simpleTrigger.Hour, _simpleTrigger.Minute,
                    _simpleTrigger.Second);
                while (nextTask - DateTime.Now < TimeSpan.Zero)
                {
                    nextTask += new TimeSpan(0, 0, 0, _simpleTrigger.RunEveryGivenSeconds);
                }

                var timeLeftTillNextTask = nextTask - DateTime.Now;
                try
                {
                    await Task.Delay(timeLeftTillNextTask, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (_simpleTrigger.ShouldRun)
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
        _simpleTrigger.ShouldRun = true;
    }

    public void Disable()
    {
        _simpleTrigger.ShouldRun = false;
    }

    public SimpleTrigger GetInternalTrigger()
    {
        return _simpleTrigger;
    }
}