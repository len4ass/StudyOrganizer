using StudyOrganizer.Services.TriggerService.Jobs;

namespace StudyOrganizer.Services.TriggerService;

public class SimpleTriggerService : IService
{
    private readonly CronJobAggregator _cronJobAggregator;

    public SimpleTriggerService(CronJobAggregator cronJobAggregator)
    {
        _cronJobAggregator = cronJobAggregator;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cronJobAggregator.QueueAllJobs(cancellationToken);
        return Task.CompletedTask;
    }
}