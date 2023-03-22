using System.Collections;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJobAggregator : IEnumerable<KeyValuePair<string, IJob>>
{
    private readonly IDictionary<string, IJob> _jobs;

    public CronJobAggregator()
    {
        _jobs = new Dictionary<string, IJob>();
    }
    
    public CronJobAggregator(IDictionary<string, IJob> jobs)
    {
        _jobs = jobs;
    }

    public void RegisterJob(string name, IJob job)
    {
        _jobs[name] = job;
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

    public IEnumerator<KeyValuePair<string, IJob>> GetEnumerator()
    {
        return _jobs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}