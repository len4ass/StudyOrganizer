namespace StudyOrganizer.Services;

public class ServiceAggregator
{
    private readonly IDictionary<string, IService> _services;
    
    public ServiceAggregator(IDictionary<string, IService> services)
    {
        _services = services;
    }

    public async Task StartAll()
    {
        foreach (var (_, service) in _services)
        {
            await service.StartAsync(GlobalCancellationToken.Cts.Token);
        }
    }
}