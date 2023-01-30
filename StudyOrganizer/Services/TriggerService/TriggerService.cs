using StudyOrganizer.Services.TriggerService.Jobs;

namespace StudyOrganizer.Services.TriggerService;

public class TriggerService : IService
{
    private readonly IDictionary<string, IJob> _crons;

    public TriggerService(IDictionary<string, IJob> crons)
    {
        _crons = crons;
    }
    
    private void StartAll()
    {
        foreach (var (_, job) in _crons)
        {
            job.Start(new CancellationToken());
        }
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartAll();
        return Task.CompletedTask;
    }
}