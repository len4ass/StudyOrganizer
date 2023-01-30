namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CustomJob : IJob
{
    private readonly CronTrigger _trigger;

    public CustomJob(CronTrigger trigger)
    {
        _trigger = trigger;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextTask = new DateTime(
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _trigger.Hour, _trigger.Minute,
                    _trigger.Second);
                if (nextTask - now < TimeSpan.Zero)
                {
                    nextTask += new TimeSpan(0, 0, 0, _trigger.RunEveryGivenSeconds);
                }

                var timeLeftTillNextTask = nextTask - now;
                await Task.Delay(timeLeftTillNextTask, cancellationToken);
                await _trigger.ExecuteAsync();
            }
        }, cancellationToken);
    }
}