namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJobAggregator
{
    private readonly IDictionary<string, IJob> _jobs;

    public CronJobAggregator(IDictionary<string, IJob> jobs)
    {
        _jobs = jobs;
    }

    public IJob? JobExists(string name)
    {
        if (_jobs.ContainsKey(name))
        {
            return _jobs[name];
        }

        return null;
    }
    
    public void QueueAllJobs(CancellationToken cancellationToken)
    {
        foreach (var (_, job) in _jobs)
        {
            job.Start(cancellationToken);
        }
    }
}