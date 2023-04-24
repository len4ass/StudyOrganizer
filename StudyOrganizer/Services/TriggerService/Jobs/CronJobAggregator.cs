using System.Collections;

namespace StudyOrganizer.Services.TriggerService.Jobs;

public class CronJobAggregator : IEnumerable<KeyValuePair<string, CronJob>>
{
    private readonly IDictionary<string, CronJob> _jobs;

    public CronJobAggregator()
    {
        _jobs = new Dictionary<string, CronJob>();
    }

    public CronJobAggregator(IDictionary<string, CronJob> jobs)
    {
        _jobs = jobs;
    }

    public void RegisterJob(string name, CronJob job)
    {
        _jobs[name] = job;
    }

    public CronJob? JobExists(string name)
    {
        if (_jobs.ContainsKey(name))
        {
            return _jobs[name];
        }

        return null;
    }

    public IEnumerator<KeyValuePair<string, CronJob>> GetEnumerator()
    {
        return _jobs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}