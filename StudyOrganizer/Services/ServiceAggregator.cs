using StudyOrganizer.Services.BotService;

namespace StudyOrganizer.Services;

public class ServiceAggregator
{
    private IDictionary<string, IService> _services;
    
    public ServiceAggregator(IDictionary<string, IService> services)
    {
        _services = services;
    }

    public async Task StartAll()
    {
        foreach (var (_, service) in _services)
        {
            await service.StartAsync(new CancellationToken());
        }
    }
}